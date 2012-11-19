using System;
using System.Dynamic;
using System.Linq;

namespace Doredis
{
    class Scope : DynamicDataObject
    {
        readonly string absolutePath;
        readonly DataStoreShard dataStore;
        static System.Reflection.MethodInfo genericGetMethod;

        static Scope()
        {
            genericGetMethod = typeof(IStructuredDataClientExtensions).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).
                Where(x => x.Name == "Get" && x.IsGenericMethod).First();
        }

        internal Scope(string absolutePath, DataStoreShard dataStore)
        {
            this.absolutePath = absolutePath;
            this.dataStore = dataStore;
        }

        internal override DataStoreShard GetDataStoreShard(string memberAbsolutePath)
        {
            return dataStore;
        }

        internal override string GetMemberAbsolutePath(string name, bool ignoreCase)
        {
            return absolutePath + "." + (ignoreCase ? name.ToLowerInvariant() : name);
        }

        internal override Scope CreateScope(string memberAbsolutePath)
        {
            return new Scope(memberAbsolutePath , dataStore);
        }

        internal override string GetAbsolutePath()
        {
            return absolutePath;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder.ReturnType == typeof(Int32))
            {
                dataStore.Set(absolutePath, (Int32)value);
                return true;
            }
            return base.TrySetMember(binder, value);
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = null;
            try
            {
                result = genericGetMethod.MakeGenericMethod(binder.ReturnType).Invoke(null, new object[] { dataStore, absolutePath });
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}
