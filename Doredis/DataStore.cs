using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Collections.Concurrent;

namespace Doredis
{
    public class DataStore : DynamicObject, IDataObject
    {
        class ShardCollection : IEnumerable<DataStoreShard>
        {
            DataStoreShard[] shards;

            internal ShardCollection(System.Net.HostEndPoint[] shardLocations)
            {
                shards = shardLocations.Select(x => new DataStoreShard(x)).ToArray();
            }

            internal DataStoreShard SelectShard(Int32 hashCode)
            {
                int shardIndex = (Int32)(((Int64)hashCode - Int32.MinValue * shards.Length) / ((Int64)Int32.MaxValue - (Int64)Int32.MinValue + 1));
                return shards[shardIndex];
            }

            public IEnumerator<DataStoreShard> GetEnumerator()
            {
                return ((System.Collections.Generic.IEnumerable<DataStoreShard>)shards).GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return shards.GetEnumerator();
            }
        }

        ShardCollection shards;

        class ScriptIdentifier
        {
            public String ScriptSource { get; private set; }
            public Type DelegateType { get; private set; }

            public ScriptIdentifier(string scriptText, Type delegateType)
            {
                ScriptSource = scriptText;
                DelegateType = delegateType;
            }

            public override bool Equals(object obj)
            {
 	                if ((Object)obj == (Object)this) return true;
                ScriptIdentifier that = obj as ScriptIdentifier;
                return this.ScriptSource == that.ScriptSource && this.DelegateType == that.DelegateType;
            }

            public override int GetHashCode()
            {
 	                return Utility.CombineHashCodes(ScriptSource, DelegateType);
            }
        }

        class ScriptBindingInformation
        {

            public string Sha1 { get; private set; }
            public Type ReturnType { get; private set; }
            public Type[] ParameterTypes { get; private set; }
            public int[] KeyParameterPositions { get; private set; }
            public int[] ArgumentParameterPositions { get; private set; }
            public bool HasCallback { get; private set; }

            public ScriptBindingInformation(Type delegateType, string scriptSource)
            {
                Sha1 = scriptSource.Utf8Sha1Hash();
                System.Reflection.MethodInfo delegateInfo = delegateType.GetMethod("Invoke");
                ReturnType = delegateInfo.ReturnType;

                List<int> keyParameterPositionsList = new List<int>();
                List<int> argumentParameterPositionsList = new List<int>();
                System.Reflection.ParameterInfo[] parameterInfos = delegateInfo.GetParameters();
                HasCallback = parameterInfos[parameterInfos.Length - 1].ParameterType == typeof(Action<RedisReply>);

                ParameterTypes = parameterInfos.Select(_ => _.ParameterType).ToArray();

                foreach (System.Reflection.ParameterInfo parameterInfo in parameterInfos)
                    if (HasCallback && parameterInfo.Position == parameterInfos.Length - 1)
                        continue;
                    else if (parameterInfo.ParameterType == typeof(IDataObject))
                        keyParameterPositionsList.Add(parameterInfo.Position);
                    else
                        argumentParameterPositionsList.Add(parameterInfo.Position);

                KeyParameterPositions = keyParameterPositionsList.ToArray();
                ArgumentParameterPositions = argumentParameterPositionsList.ToArray();
            }

        }

        ConcurrentDictionary<ScriptIdentifier, ScriptBindingInformation> scriptCache;

        static System.Reflection.MethodInfo executeScriptCallMethodInfo = typeof(DataStore).GetMethod("ExecuteScriptCall", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        public DataStore(System.Net.HostEndPoint[] shardLocations)
        {
            shards = new ShardCollection(shardLocations);
            scriptCache = new ConcurrentDictionary<ScriptIdentifier, ScriptBindingInformation>();
        }

        DataStoreShard IDataObject.GetDataStoreShard(string memberAbsolutePath)
        {
            return shards.SelectShard(memberAbsolutePath.Split(new char[] { '.' })[0].GetHashCode());
        }

        string IDataObject.GetMemberAbsolutePath(string name, bool ignoreCase)
        {
            return ignoreCase ? name.ToLowerInvariant() : name;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = ((IDataObject)this).CreateMember(binder.Name, binder.IgnoreCase);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            ((IDataObject)this).AssignMember(binder.Name, binder.IgnoreCase, value);
            return true;
       }

        Scope CreateScope(string memberName)
        {
            return new Scope(memberName, ((IDataObject)this).GetDataStoreShard(memberName));
        }

        string IDataObject.GetAbsolutePath()
        {
            throw new InvalidOperationException("The top level data store object cannot be assigned to.");
        }

        System.Net.HostEndPoint IDataObject.EndPoint
        {
            get
            {
                throw new InvalidOperationException("The top level data store object does not have an endpoint.");
            }
        }

        Object ExecuteScriptCall(ScriptBindingInformation info, Object[] parameters)
        {
            List<Object> arrangedCommandParameters = new List<Object>();
            arrangedCommandParameters.Add(info.Sha1);
            arrangedCommandParameters.Add(info.KeyParameterPositions.Length);
            string rootScope = null;
            for (int keyIndex = 0; keyIndex < info.KeyParameterPositions.Length; keyIndex++)
            {
                int keyParameterPosition = info.KeyParameterPositions[keyIndex];
                arrangedCommandParameters.Add(((IDataObject)parameters[keyParameterPosition]).GetAbsolutePath());
                if (keyIndex == 0)
                {
                    rootScope = arrangedCommandParameters[2].ToString().Split(new char[] { '.' })[0];
                }
                else if (arrangedCommandParameters[keyIndex].ToString().Split(new char[] { '.' })[0] != rootScope)
                {
                    throw new KeysDoNotHaveIdenticalRootScope();
                }
            }
            for (int argumentIndex = 0; argumentIndex < info.ArgumentParameterPositions.Length; argumentIndex++)
            {
                int argumentParameterPosition = info.ArgumentParameterPositions[argumentIndex];
                arrangedCommandParameters.Add(parameters[argumentParameterPosition]);
            }
            DataStoreShard shard = shards.SelectShard(rootScope.GetHashCode());
            if (info.HasCallback)
            {
                shard.CommandWithPackedParameters("EVALSHA", arrangedCommandParameters.ToArray(), (Action<RedisReply>)parameters[parameters.Length - 1]);
                return null;
            }
            else
            {
                return shard.CommandWithPackedParameters(info.ReturnType, "EVALSHA", arrangedCommandParameters.ToArray());
            }
        }

        ScriptBindingInformation CreateScriptBinding(ScriptIdentifier scriptIdentifier)
        {
            ScriptBindingInformation scriptBindingInformation = new ScriptBindingInformation(scriptIdentifier.DelegateType, scriptIdentifier.ScriptSource);
            foreach (DataStoreShard shard in shards)
                shard.Command<string>("SCRIPT", "LOAD", scriptIdentifier.ScriptSource);
            return scriptBindingInformation;
        }

        internal DelegateType CreateScript<DelegateType>(string scriptSource)
        {
            ScriptIdentifier scriptIdentifier = new ScriptIdentifier(scriptSource, typeof(DelegateType));
            ScriptBindingInformation scriptBindingInformation = scriptCache.GetOrAdd(scriptIdentifier, _ => { return CreateScriptBinding(_);});
            System.Linq.Expressions.ParameterExpression[] parameterExpressions = 
                scriptBindingInformation.ParameterTypes.Select(_ => System.Linq.Expressions.Expression.Parameter(_)).ToArray();
            System.Linq.Expressions.Expression expression = System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.Constant(this, typeof(DataStore)),
                executeScriptCallMethodInfo,
                System.Linq.Expressions.Expression.Constant(scriptBindingInformation, typeof(ScriptBindingInformation)),
                System.Linq.Expressions.Expression.NewArrayInit(typeof(Object),
                    parameterExpressions.Select(_ => System.Linq.Expressions.Expression.Convert(_, typeof(Object))))
            );

            System.Linq.Expressions.Expression<DelegateType> lambda = System.Linq.Expressions.Expression.Lambda<DelegateType>(
                scriptBindingInformation.ReturnType == typeof(void) ?
                expression :
                System.Linq.Expressions.Expression.Convert(expression, scriptBindingInformation.ReturnType),
                parameterExpressions
            );
            return lambda.Compile();
        }

    }

    [Serializable]
    public class KeysDoNotHaveIdenticalRootScope : Exception
    {
        public KeysDoNotHaveIdenticalRootScope() : base("Keys must have the same root scope (all characters before the first period) to ensure they will reside on the same server.") { }
        public KeysDoNotHaveIdenticalRootScope(string message) : base(message) { }
        public KeysDoNotHaveIdenticalRootScope(string message, Exception inner) : base(message, inner) { }
        protected KeysDoNotHaveIdenticalRootScope(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
