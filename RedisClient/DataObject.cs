using System;
using System.Dynamic;
using System.Runtime.Serialization.Formatters.Binary;

namespace Redis
{
    public abstract class DynamicDataObject : DynamicObject, IDataObject
    {      

        protected DynamicDataObject()
        {
        }

        internal abstract Scope CreateScope(string memberName);

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string memberName = GetMemberAbsolutePath(binder.Name, binder.IgnoreCase);
            ServiceStack.Redis.RedisNativeClient dataStore = GetDataStore(memberName);
            if (dataStore.Exists(memberName) == 1)
            {
                switch (dataStore.GetEntryType(memberName))
                {
                    case ServiceStack.Redis.RedisKeyType.List:

                        break;
                    default:
                        throw new NotImplementedException();
                }
                //there's actually a data object for this
                result = SerializationProvider.Deserialize(dataStore.Get(memberName));
            }
            else
            {
                //treat it as another scope
                result = CreateScope(memberName);
            }
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            string memberName = GetMemberAbsolutePath(binder.Name, binder.IgnoreCase);
            ServiceStack.Redis.RedisNativeClient dataStore = GetDataStore(memberName);
            dataStore.Set(memberName, SerializationProvider.Serialize(value));
            return true;
        }

        public abstract ServiceStack.Redis.RedisClient GetDataStore(string memberAbsolutePath);

        public abstract string GetMemberAbsolutePath(string name, bool ignoreCase);
    }
}
