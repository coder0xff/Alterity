using System;
using System.Dynamic;
using System.Runtime.Serialization.Formatters.Binary;

namespace Doredis
{
    internal abstract class DynamicDataObject : DynamicObject, ILockableDataObject
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
                object result;
                string memberName = GetMemberAbsolutePath(name, false);
                IStructuredDataClient dataStore = GetDataStore(memberName);
                if (dataStore.Exists(memberName))
                {
//                     switch (dataStore.GetEntryType(memberName))
//                     {
//                         case ServiceStack.Redis.RedisKeyType.List:
// 
//                             break;
//                         default:
                            throw new NotImplementedException();
//                     }
                    //there's actually a data object for this
//                    result = dataStore.Get(memberName);
                }
                else
                {
                    //treat it as another scope
                    result = CreateScope(memberName);
                }
                return result;
            }
            set
            {
                string memberName = GetMemberAbsolutePath(name, false);
                DataStoreShard dataStore = GetDataStore(memberName);
                dataStore.Set(memberName, SerializationProvider.Serialize(value));
            }
        }

        public abstract DataStoreShard GetDataStore(string memberAbsolutePath);

        public abstract string GetMemberAbsolutePath(string name, bool ignoreCase);

        public abstract string GetAbsolutePath();

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
