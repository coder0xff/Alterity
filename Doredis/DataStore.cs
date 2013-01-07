using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;

namespace Doredis
{
    public class DataStore : DynamicObject, IDataObject
    {
        private static readonly MethodInfo ExecuteScriptCallMethodInfo =
            typeof (DataStore).GetMethod("ExecuteScriptCall", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly ConcurrentDictionary<ScriptIdentifier, ScriptBindingInformation> _boundScripts;

        private readonly ShardCollection _shardCollection;

        private readonly ConcurrentSet<string> _uploadedScriptHashes;

        public DataStore(IEnumerable<HostEndPoint> shardLocations)
        {
            _shardCollection = new ShardCollection(this, shardLocations);
            _uploadedScriptHashes = new ConcurrentSet<string>();
            _boundScripts = new ConcurrentDictionary<ScriptIdentifier, ScriptBindingInformation>();
        }

        DataStoreShard IDataObject.GetDataStoreShard(string memberAbsolutePath)
        {
            return _shardCollection.SelectShard(memberAbsolutePath.Split(new[] {'.'})[0].GetHashCode());
        }

        string IDataObject.GetMemberAbsolutePath(string name, bool ignoreCase)
        {
            return ignoreCase ? name.ToLowerInvariant() : name;
        }

        string IDataObject.GetAbsolutePath()
        {
            throw new InvalidOperationException("The top level data store object cannot be assigned to.");
        }

        HostEndPoint IDataObject.EndPoint
        {
            get { throw new InvalidOperationException("The top level data store object does not have an endpoint."); }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this.CreateMember(binder.Name, binder.IgnoreCase);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this.AssignMember(binder.Name, binder.IgnoreCase, value);
            return true;
        }

// ReSharper disable UnusedMember.Local
        /// <summary>
        ///     This method is called using reflection
        /// </summary>
        /// <param name="info"></param>
        /// <param name="parameters"></param>
        /// <exception cref="NoKeysException"></exception>
        /// <returns></returns>
        private Object ExecuteScriptCall(ScriptBindingInformation info, Object[] parameters)
// ReSharper restore UnusedMember.Local
        {
            var arrangedCommandParameters = new List<Object> {info.Sha1, info.KeyParameterPositions.Length};
            string rootScope = null;
            for (int keyIndex = 0; keyIndex < info.KeyParameterPositions.Length; keyIndex++)
            {
                int keyParameterPosition = info.KeyParameterPositions[keyIndex];
                arrangedCommandParameters.Add(((IDataObject) parameters[keyParameterPosition]).GetAbsolutePath());
                if (keyIndex == 0)
                {
                    rootScope = arrangedCommandParameters[2].ToString().Split(new[] {'.'})[0];
                }
                else if (arrangedCommandParameters[keyIndex].ToString().Split(new[] {'.'})[0] != rootScope)
                {
                    throw new KeysDoNotHaveIdenticalRootScope();
                }
            }
            arrangedCommandParameters.AddRange(
                info.ArgumentParameterPositions.Select(
                    argumentParameterPosition => parameters[argumentParameterPosition]));
            if (rootScope == null) throw new NoKeysException();
            DataStoreShard shard = _shardCollection.SelectShard(rootScope.GetHashCode());
            if (info.HasCallback)
            {
                shard.CommandWithPackedParameters("EVALSHA", arrangedCommandParameters.ToArray(),
                                                  (Action<RedisReply>) parameters[parameters.Length - 1]);
                return null;
            }
            return shard.CommandWithPackedParameters(info.ReturnType, "EVALSHA", arrangedCommandParameters.ToArray());
        }

        internal void UploadScript(string scriptSource, string sha1 = null)
        {
            if (sha1 == null) sha1 = scriptSource.Utf8Sha1Hash();
            if (_uploadedScriptHashes.TryAdd(sha1))
                foreach (DataStoreShard shard in _shardCollection)
                    shard.Command<string>("SCRIPT", "LOAD", scriptSource);
        }

        private ScriptBindingInformation CreateScriptBinding(ScriptIdentifier scriptIdentifier)
        {
            var scriptBindingInformation = new ScriptBindingInformation(scriptIdentifier.DelegateType,
                                                                        scriptIdentifier.ScriptSource);
            UploadScript(scriptIdentifier.ScriptSource, scriptBindingInformation.Sha1);
            return scriptBindingInformation;
        }

        internal TDelegate CreateScriptLambda<TDelegate>(string scriptSource)
        {
            var scriptIdentifier = new ScriptIdentifier(scriptSource, typeof (TDelegate));
            ScriptBindingInformation scriptBindingInformation = _boundScripts.GetOrAdd(scriptIdentifier,
                                                                                       CreateScriptBinding);
            ParameterExpression[] parameterExpressions =
                scriptBindingInformation.ParameterTypes.Select(Expression.Parameter).ToArray();
            Expression expression = Expression.Call(
                Expression.Constant(this, typeof (DataStore)),
                ExecuteScriptCallMethodInfo,
                Expression.Constant(scriptBindingInformation, typeof (ScriptBindingInformation)),
                Expression.NewArrayInit(typeof (Object),
                                        parameterExpressions.Select(_ => Expression.Convert(_, typeof (Object))))
                );

            Expression<TDelegate> lambda = Expression.Lambda<TDelegate>(
                scriptBindingInformation.ReturnType == typeof (void)
                    ? expression
                    : Expression.Convert(expression, scriptBindingInformation.ReturnType),
                parameterExpressions
                );
            return lambda.Compile();
        }

        internal DataStoreShard LockConductorShard()
        {
            return _shardCollection.SelectShard(0);
        }

        private class ScriptBindingInformation
        {
            public ScriptBindingInformation(Type delegateType, string scriptSource)
            {
                Sha1 = scriptSource.Utf8Sha1Hash();
                MethodInfo delegateInfo = delegateType.GetMethod("Invoke");
                ReturnType = delegateInfo.ReturnType;

                var keyParameterPositionsList = new List<int>();
                var argumentParameterPositionsList = new List<int>();
                ParameterInfo[] parameterInfos = delegateInfo.GetParameters();
                HasCallback = parameterInfos[parameterInfos.Length - 1].ParameterType == typeof (Action<RedisReply>);

                ParameterTypes = parameterInfos.Select(_ => _.ParameterType).ToArray();

                foreach (ParameterInfo parameterInfo in parameterInfos)
                    if (HasCallback && parameterInfo.Position == parameterInfos.Length - 1)
                    {
                    }
                    else if (parameterInfo.ParameterType == typeof (IDataObject))
                        keyParameterPositionsList.Add(parameterInfo.Position);
                    else
                        argumentParameterPositionsList.Add(parameterInfo.Position);

                KeyParameterPositions = keyParameterPositionsList.ToArray();
                ArgumentParameterPositions = argumentParameterPositionsList.ToArray();
            }

            public string Sha1 { get; private set; }
            public Type ReturnType { get; private set; }
            public Type[] ParameterTypes { get; private set; }
            public int[] KeyParameterPositions { get; private set; }
            public int[] ArgumentParameterPositions { get; private set; }
            public bool HasCallback { get; private set; }
        }

        private class ScriptIdentifier
        {
            public ScriptIdentifier(string scriptText, Type delegateType)
            {
                ScriptSource = scriptText;
                DelegateType = delegateType;
            }

            public String ScriptSource { get; private set; }
            public Type DelegateType { get; private set; }

            public override bool Equals(object obj)
            {
                if (obj == this) return true;
                var that = obj as ScriptIdentifier;
                if (that == null) return false;
                return ScriptSource == that.ScriptSource && DelegateType == that.DelegateType;
            }

            public override int GetHashCode()
            {
                return Utility.CombineHashCodes(ScriptSource, DelegateType);
            }
        }

        private class ShardCollection : IEnumerable<DataStoreShard>
        {
            private readonly DataStoreShard[] _shards;

            internal ShardCollection(DataStore owner, IEnumerable<HostEndPoint> shardLocations)
            {
                _shards = shardLocations.Select(x => new DataStoreShard(owner, x)).ToArray();
            }

            public IEnumerator<DataStoreShard> GetEnumerator()
            {
                return ((IEnumerable<DataStoreShard>) _shards).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _shards.GetEnumerator();
            }

            internal DataStoreShard SelectShard(Int32 hashCode)
            {
                var shardIndex =
                    (Int32)
                    (((Int64) hashCode - Int32.MinValue*_shards.Length)/(Int32.MaxValue - (Int64) Int32.MinValue + 1));
                return _shards[shardIndex];
            }
        }
    }

    [Serializable]
    class NoKeysException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public NoKeysException()
        {
        }

        public NoKeysException(string message)
            : base(message)
        {
        }

        public NoKeysException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected NoKeysException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class KeysDoNotHaveIdenticalRootScope : Exception
    {
        public KeysDoNotHaveIdenticalRootScope()
            : base(
                "Keys must have the same root scope (all characters before the first period) to ensure they will reside on the same server."
                )
        {
        }

        public KeysDoNotHaveIdenticalRootScope(string message) : base(message)
        {
        }

        public KeysDoNotHaveIdenticalRootScope(string message, Exception inner) : base(message, inner)
        {
        }

        protected KeysDoNotHaveIdenticalRootScope(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}