using System;
using System.Dynamic;
using System.Linq;

namespace Doredis
{
    class Scope : DynamicObject, IDataObject
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

        Scope CreateScope(string memberAbsolutePath)
        {
            return new Scope(memberAbsolutePath, dataStore);
        }

        DataStoreShard IDataObject.GetDataStoreShard(string memberAbsolutePath)
        {
            return dataStore;
        }

        string IDataObject.GetMemberAbsolutePath(string name, bool ignoreCase)
        {
            return absolutePath + "." + (ignoreCase ? name.ToLowerInvariant() : name);
        }

        string IDataObject.GetAbsolutePath()
        {
            return absolutePath;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this.TryGetStaticallyTypedMember(binder, out result))
                return true;

            result = ((IDataObject)this).CreateMember(binder.Name, binder.IgnoreCase);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (this.TrySetStaticallyTypedMember(binder, value))
                return true;

            ((IDataObject)this).AssignMember(binder.Name, binder.IgnoreCase, value);
            return true;
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

        public long Increment()
        {
            return dataStore.Increment(absolutePath);
        }

        System.Net.HostEndPoint IDataObject.EndPoint
        {
            get { return ((IDataObject)this).GetDataStoreShard("").EndPoint; }
        }
    }
}
