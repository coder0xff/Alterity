using System;
using System.Dynamic;
using System.Linq;

namespace Doredis
{
    class Scope : DynamicObject, ILockableDataObject
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

        DataStoreShard IPathObject.GetDataStoreShard(string memberAbsolutePath)
        {
            return dataStore;
        }

        string IPathObject.GetMemberAbsolutePath(string name, bool ignoreCase)
        {
            return absolutePath + "." + (ignoreCase ? name.ToLowerInvariant() : name);
        }

        string IPathObject.GetAbsolutePath()
        {
            return absolutePath;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this.TryGetStaticallyTypedMember(binder, out result))
                return true;

            result = ((IPathObject)this).CreateMember(binder.Name, binder.IgnoreCase);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (this.TrySetStaticallyTypedMember(binder, value))
                return true;

            ((IPathObject)this).AssignMember(binder.Name, binder.IgnoreCase, value);
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

        // this class is not meant to be directly atomic (they are indirectly)
        // it's just a way to keep track of objects that are locking this class
        // to be consistent through their use (ie. enumerators)
        volatile Lock lockObject;

        void ILockable.SetLock(Lock lockObject)
        {
            this.lockObject = lockObject;
        }

        void ILockable.ClearLock()
        {
            this.lockObject = null;
        }

        Lock ILockable.GetLock()
        {
            return lockObject;
        }

        public long Increment()
        {
            return dataStore.Increment(absolutePath);
        }
    }
}
