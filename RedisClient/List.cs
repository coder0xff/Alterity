using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis
{
    class List : IList, IDataObject
    {
        internal ServiceStack.Redis.RedisClient dataStore;
        internal string absolutePath;

        public ServiceStack.Redis.RedisClient GetDataStore(string memberAbsolutePath)
        {
            return dataStore;
        }

        public string GetMemberAbsolutePath(string name, bool ignoreCase)
        {
            return absolutePath + "." + (ignoreCase ? name.ToLowerInvariant() : name);
        }


        public int Add(object value)
        {
            int Count = 0;
            Lock.On(this, System.Threading.Timeout.Infinite, () =>
            {
                dataStore.RPush(absolutePath, SerializationProvider.Serialize(value));
                Count = dataStore.LLen(absolutePath);
            });
            return Count;
        }

        public void Clear()
        {
            dataStore.Del(absolutePath);
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public void Remove(object value)
        {
            dataStore.LRem(absolutePath, 1, SerializationProvider.Serialize(value));
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public object this[int index]
        {
            get
            {
                return SerializationProvider.Deserialize(dataStore.LIndex(absolutePath, index));
            }
            set
            {
                dataStore.LSet(absolutePath, index, SerializationProvider.Serialize(value));
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return dataStore.LLen(absolutePath); }
        }

        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

    }
}
