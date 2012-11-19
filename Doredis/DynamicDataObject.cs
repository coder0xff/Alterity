using System;
using System.Dynamic;
using System.Runtime.Serialization.Formatters.Binary;

namespace Doredis
{
    public abstract class DynamicDataObject : DynamicObject, ILockableDataObject
    {      
        protected DynamicDataObject()
        {
        }

        internal abstract Scope CreateScope(string memberName);

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.IgnoreCase ? binder.Name.ToLowerInvariant() : binder.Name];
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.IgnoreCase ? binder.Name.ToLowerInvariant() : binder.Name] = value;
            return true;
        }

        public object this[string name]
        {
            get
            {
                string memberName = GetMemberAbsolutePath(name, false);
                IStructuredDataClient dataStore = GetDataStoreShard(memberName);
                return CreateScope(memberName);
            }
            set
            {
                string memberName = GetMemberAbsolutePath(name, false);
                DataStoreShard dataStore = GetDataStoreShard(memberName);
                dataStore.Set(memberName, value);
            }
        }

        internal abstract DataStoreShard GetDataStoreShard(string memberAbsolutePath);
        DataStoreShard IDataObject.GetDataStoreShard(string memberAbsolutePath) { return GetDataStoreShard(memberAbsolutePath); }

        internal abstract string GetMemberAbsolutePath(string name, bool ignoreCase);
        string IDataObject.GetMemberAbsolutePath(string name, bool ignoreCase) { return GetMemberAbsolutePath(name, ignoreCase); }

        internal abstract string GetAbsolutePath();
        string IDataObject.GetAbsolutePath() { return GetAbsolutePath(); }

        // this is not meant to be directly atomic (they are indirectly)
        // it's just a way to keep track of objects that expect a lock
        // to be consistently held through their use (ie. enumerators)
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
    }
}
