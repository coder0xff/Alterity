using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis
{
    public class List : IList, ILockableDataObject
    {
        class Enumerator : IEnumerator
        {
            const int bufferSize = 100;
            List list;
            Lock requiredLock;
            System.Threading.Thread requiredThread;
            Object[] buffer = new Object[bufferSize];
            int bufferReadIndex = -1;
            int retrieveIndex = 0;
            int bufferEndIndex = 0;

            void CheckIntegrity()
            {
                if (System.Threading.Thread.CurrentThread != requiredThread) throw new InvalidOperationException("This enumerator can only be used on the thread that it was created on.");
                if (((ILockable)list).GetLock() != requiredLock) throw new InvalidOperationException("This enumerator can only be used inside the Lock.On in which it was created.");
            }

            void RetrieveObjects()
            {
                byte[][] objects = list.dataStore.LRange(list.absolutePath, retrieveIndex, retrieveIndex + bufferSize - 1);
                for (int deserialize = 0; deserialize < objects.Length; deserialize++)
                {
                    buffer[deserialize] = SerializationProvider.Deserialize(objects[deserialize]);
                }
                bufferReadIndex = 0;
                bufferEndIndex = objects.Length;
                retrieveIndex += objects.Length;
            }

            public Enumerator(List list)
            {
                this.list = list;
                requiredLock = ((ILockable)list).GetLock();
                if (requiredLock == null) throw new InvalidOperationException("Redis list enumerators can only be used inside of a lock on the list.");
                requiredThread = requiredLock.GetRequiredThread();
            }

            public object Current
            {
                get
                {
                    CheckIntegrity();
                    if (bufferReadIndex == -1) return null; //specs say the return value is undefined 
                    if (bufferReadIndex >= bufferEndIndex) throw new InvalidOperationException("The enumerator has passed the element.");
                    return buffer[bufferReadIndex];
                }
            }

            public bool MoveNext()
            {
                CheckIntegrity();
                if (bufferReadIndex == bufferSize - 1 || bufferReadIndex == -1)
                {
                    RetrieveObjects();
                    return bufferReadIndex < bufferEndIndex;
                }
                else
                {
                    if (bufferReadIndex == bufferEndIndex)
                    {
                        return false;
                    }
                    else
                    {
                        bufferReadIndex++;
                        if (bufferReadIndex == bufferEndIndex)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            public void Reset()
            {
                CheckIntegrity();
                bufferReadIndex = -1;
                retrieveIndex = 0;
            }
        }

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

        internal List(ServiceStack.Redis.RedisClient dataStore, string absolutePath)
        {
            this.dataStore = dataStore;
            this.absolutePath = absolutePath;
        }

        public int Add(object value)
        {
            int Count = 0;
            Lock.On(this, () =>
            {
                Count = dataStore.RPush(absolutePath, SerializationProvider.Serialize(value)) - 1;
            });
            return Count;
        }

        public void Clear()
        {
            Lock.On(this, () =>
            {
                dataStore.Del(absolutePath);
            });
        }

        public bool Contains(object value)
        {
            bool result = false;
            Lock.On(this, () =>
            {
                foreach (object other in this)
                    if (value.Equals(other))
                    {
                        result = true;
                        break;
                    }
            });
            return result;
        }

        public int IndexOf(object value)
        {
            int result = -1;
            Lock.On(this, () =>
            {
                int index = 0;
                foreach (object other in this)
                {
                    if (value.Equals(other))
                    {
                        result = index;
                        break;
                    }
                    index++;
                }
            });
            return result;
        }

        public void Insert(int index, object value)
        {
            Lock.On(this, () =>
            {
                if (index == 0)
                {
                    dataStore.LPush(absolutePath, SerializationProvider.Serialize(value));
                }
                else if (index == this.Count)
                {
                    dataStore.RPush(absolutePath, SerializationProvider.Serialize(value));
                }
                else
                {
                    string[] subsequentData = dataStore.GetRangeFromList(absolutePath, index, -1).ToArray();
                    dataStore.LTrim(absolutePath, 0, index - 1);
                    dataStore.RPush(absolutePath, SerializationProvider.Serialize(value));
                    using (var trans = dataStore.CreateTransaction())
                    {
                        for (int itemIndex = 0; itemIndex < subsequentData.Length; itemIndex++)
                            trans.QueueCommand(x => x.PushItemToList(absolutePath, subsequentData[itemIndex]));

                        trans.Commit();
                    }
                }
            });
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            Lock.On(this, () =>
            {
                dataStore.LRem(absolutePath, 1, SerializationProvider.Serialize(value));
            });
        }

        public void RemoveAt(int index)
        {
            string removeMarker = "Some ridiculous and random string, marking an item for deletion, that will hopefully go unnoticed.";
            Lock.On(this, () =>
            {
                dataStore.SetItemInList(absolutePath, index, removeMarker);
                dataStore.RemoveItemFromList(absolutePath, removeMarker);
            });
        }

        public object this[int index]
        {
            get
            {
                object result = null;
                Lock.On(this, () =>
                {
                    result = SerializationProvider.Deserialize(dataStore.LIndex(absolutePath, index));
                });
                return result;
            }
            set
            {
                Lock.On(this, () =>
                {
                    dataStore.LSet(absolutePath, index, SerializationProvider.Serialize(value));
                });
           }
        }

        public void CopyTo(Array array, int index)
        {
            Lock.On(this, () =>
            {
                foreach (object other in this)
                {
                    array.SetValue(other, index++);
                }
            });
        }

        public int Count
        {
            get { return dataStore.LLen(absolutePath); }
        }

        public bool IsSynchronized
        {
            get { return true; }
        }

        public object SyncRoot
        {
            get { throw new InvalidOperationException(); }
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

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

        public string GetAbsolutePath()
        {
            return absolutePath;
        }
    }
}
