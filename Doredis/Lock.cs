using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Doredis
{
    interface ILockable
    {
        void SetLock(Lock lockObject);
        void ClearLock();
        Lock GetLock();
    }

    public class Lock: IDisposable
    {
        static int keepAliveExpireTime = 10; //if the server that held the lock goes down, this'll keep all the other servers from waiting forever

        class ClientEntry : IDisposable
        {
            DataStoreShard shard;
            Action<string> signalHandler;
            public List<ILockableDataObject> dataObjects = new List<ILockableDataObject>();
            /// <summary>
            /// used to make sure that this client entry doesn't react to its own signals
            /// </summary>
            public Guid guid = Guid.NewGuid();
            public ClientEntry(DataStoreShard shard, Action<string> signalHandler)
            {
                this.shard = shard;
                this.signalHandler = signalHandler;
            }

            public void Add(ILockableDataObject dataObject)
            {
                shard.Subscribe(dataObject.GetAbsolutePath(), signalHandler);
                dataObjects.Add(dataObject);
            }

            string GetLockPath(ILockableDataObject dataObject)
            {
                return dataObject.GetAbsolutePath() + ".__lock__";
            }

            string GetLockCountPath(ILockableDataObject dataObject)
            {
                return dataObject.GetAbsolutePath() + ".__lock_count__";
            }

            bool[] AllIndicesTrue()
            {
                bool[] result = new bool[dataObjects.Count];
                for (int index = 0; index < dataObjects.Count; index++)
                    result[index] = true;
                return result;
            }

            void IncrementLockCounts(bool[] forceToOneIndices)
            {
                shard.Transaction(trans =>
                {
                    for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    {
                        ILockableDataObject dataObject = dataObjects[dataObjectIndex];
                        string lockCountPath = GetLockCountPath(dataObject);
                        if (forceToOneIndices[dataObjectIndex])
                            trans.Set(lockCountPath, 1, r => r.Expect<OkReply>());
                        else
                            trans.Increment(lockCountPath, r => r.Expect<long>());
                    }                    
                });
            }

            bool[] DecrementLockCounts()
            {
                bool[] result = new bool[dataObjects.Count];
                shard.Transaction(trans =>
                {
                    for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    {
                        int dataObjectIndexCopy = dataObjectIndex;
                        ILockableDataObject dataObject = dataObjects[dataObjectIndexCopy];
                        string lockCountPath = GetLockCountPath(dataObject);
                        trans.Decrement(lockCountPath, _ => result[dataObjectIndexCopy] = _.Expect<long>() == 0);
                    }
                });

                return result;
            }

            public void ClearLocks(bool[] markedIndices)
            {
                shard.Transaction(trans =>
                {
                    for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    {
                        if (markedIndices[dataObjectIndex])
                        {
                            ILockableDataObject dataObject = dataObjects[dataObjectIndex];
                            string atomicLockPath = GetLockPath(dataObject);
                            trans.Set(atomicLockPath, 0, r => r.Expect<OkReply>());
                        }
                    }
                });
            }

            public void SignalUnlocks(bool[] markedIndices)
            {
                shard.Transaction(trans => 
                {
                    for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    {
                        if (markedIndices[dataObjectIndex])
                        {
                            ILockableDataObject dataObject = dataObjects[dataObjectIndex];
                            trans.Publish(dataObject.GetAbsolutePath(), guid.ToString(), r => r.Expect<int>());
                        }
                    }
                });
            }

            public bool TryLockAll(out bool[] neededUnlocks)
            {
                bool[] successes = new bool[dataObjects.Count];
                // make sure we reset the count on new locks, in case a lock was abandoned
                bool[] isNewLock = new bool[dataObjects.Count];
                bool canceled = false;

                shard.Transaction(trans =>
                {
                    for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    {
                        ILockableDataObject dataObject = dataObjects[dataObjectIndex];
                        string atomicLockPath = GetLockPath(dataObject);

                        Lock priorLock = dataObject.GetLock();
                        if (priorLock != null)
                        {
                            if (priorLock.requiredThread == System.Threading.Thread.CurrentThread)
                            {
                                // it's already locked on this thread, so automatic success
                                successes[dataObjectIndex] = true;
                            }
                            else
                            {
                                // this object is already locally locked, and not by us
                                // so don't even bother talking to the server
                                throw trans.Cancel();
                            }
                        }
                        else
                        {
                            int dataObjectIndexCopy = dataObjectIndex;
                            trans.Get(atomicLockPath, r => successes[dataObjectIndexCopy] = isNewLock[dataObjectIndexCopy] = r.Expect<long>() == 0);
                            trans.Set(atomicLockPath, 1, r => r.Expect<OkReply>());
                            trans.Expire(atomicLockPath, Lock.keepAliveExpireTime, r => { if (r.Expect<long>() != 1) throw new RequestFailedException(); });
                        }
                    }
                });
                neededUnlocks = new bool[dataObjects.Count];
                if (canceled) return false;

                if (!successes.All(_ => _))
                {
                    //we didn't succeed overall, but we still have to unlock any that did
                    neededUnlocks = isNewLock;
                    ClearLocks(isNewLock);
                    SignalUnlocks(isNewLock);
                    return false;
                }
                else
                {
                    // success
                    neededUnlocks = new bool[dataObjects.Count];
                    foreach (ILockableDataObject dataObject in dataObjects)
                    {
                        shard.Unsubscribe(dataObject.GetAbsolutePath(), signalHandler);
                    }
                    IncrementLockCounts(isNewLock);
                    return true;
                }
            }

            public void KeepAliveAll()
            {
                shard.Transaction(trans =>
                {
                    for (int objectPathIndex = 0; objectPathIndex < dataObjects.Count; objectPathIndex++)
                    {
                        string objectPath = GetLockPath(dataObjects[objectPathIndex]);
                        trans.Expire(objectPath, Lock.keepAliveExpireTime);
                    }
                });
            }

            public bool[] UnlockAll()
            {
                bool[] trulyUnlocked = DecrementLockCounts();
                ClearLocks(trulyUnlocked);
                return trulyUnlocked;
            }

            public void Dispose()
            {
                dataObjects.Clear();
            }
        }

        readonly ManualResetEvent signal;
        readonly Dictionary<DataStoreShard, ClientEntry> objectsPaths = new Dictionary<DataStoreShard, ClientEntry>();

        readonly Thread requiredThread;
        bool isWaiting;
        Object syncRoot;
        bool hasLock;
        bool succeeded;
        Timer keepAliveTimer;

        public static bool On(object dataObject, Action action)
        {
            return On(dataObject, Timeout.Infinite, action);
        }

        public static bool On(object dataObject, int timeoutMilliseconds, Action action)
        {
            return On(new object[] { dataObject }, timeoutMilliseconds, action);
        }

        public static bool On(object[] dataObjects, int timeoutMilliseconds, Action action)
        {
            return (new Lock(dataObjects, timeoutMilliseconds, action)).succeeded;
        }

        Lock(object[] dataObjects, int timeoutMilliseconds, Action action)
        {
            foreach (object dataObject in dataObjects)
                if (!(dataObject is IDataObject))
                    throw new ArgumentException("All the objects must be dynamic objects created by Doredis");

            requiredThread = System.Threading.Thread.CurrentThread;
            succeeded = false;
            hasLock = false;
            isWaiting = true;
            syncRoot = 0; //just a boxed dummy value
            signal = new ManualResetEvent(true);
            Timer timeoutTimer = new Timer(_ => Fail(), null, timeoutMilliseconds, Timeout.Infinite);
            try
            {
                foreach (ILockableDataObject dataObject in dataObjects)
                {
                    DataStoreShard dataStore = dataObject.GetDataStoreShard(dataObject.GetAbsolutePath());
                    if (!objectsPaths.ContainsKey(dataStore)) objectsPaths[dataStore] = new ClientEntry(dataStore, (string dontCare) => signal.Set());
                    objectsPaths[dataStore].Add(dataObject);
                }
                syncRoot = 0;
                while (true)
                {
                    try
                    {
                        lock (syncRoot)
                        {
                            signal.Reset();
                            if (isWaiting)
                            {
                                hasLock = TryAttainLock();
                                if (hasLock)
                                {
                                    isWaiting = false;
                                }
                            }
                            else
                                break;
                        }
                        if (hasLock)
                        {
                            foreach (ILockableDataObject dataObject in dataObjects)
                                dataObject.SetLock(this);
                            succeeded = true;
                            action();
                            foreach (ILockableDataObject dataObject in dataObjects)
                                dataObject.ClearLock();
                        }
                    }
                    finally
                    {
                        lock (syncRoot)
                        {
                            if (hasLock)
                            {
                                hasLock = false;
                                FreeLock();
                            }
                        }
                    }
                    //if the server that held the lock goes down, this'll prevent all the others from waiting forever
                    //eventually, the lock will expire
                    signal.WaitOne(keepAliveExpireTime * 1000);
                }
            }
            finally
            {
                foreach (var entry in objectsPaths)
                {
                    entry.Value.Dispose();
                }
                timeoutTimer.Dispose();
                signal.Dispose();
            }
        }

        void Fail()
        {
            lock (syncRoot)
            {
                if (!isWaiting) return;
                isWaiting = false;
                signal.Set();
            }
        }

        void PreserverLock()
        {
            lock (syncRoot)
            {
                if (hasLock)
                {
                    foreach (var entry in objectsPaths)
                    {
                        entry.Value.KeepAliveAll();
                    }
                }
            }
        }

        bool TryAttainLock()
        {
            ClientEntry[] clients = objectsPaths.Values.ToArray();
            bool[] clientSuccessFlags = new bool[objectsPaths.Count];
            for (int tryLockClientIndex = 0; tryLockClientIndex < clients.Length; tryLockClientIndex++)
            {
                bool[] newLockWasCreatedByUnsuccessfulClient;
                if (clients[tryLockClientIndex].TryLockAll(out newLockWasCreatedByUnsuccessfulClient))
                {
                    clientSuccessFlags[tryLockClientIndex] = true;
                }
                else
                {
                    clients[tryLockClientIndex].ClearLocks(newLockWasCreatedByUnsuccessfulClient);
                    bool[][] newLockWasCreatedBySuccessfulClient = new bool[tryLockClientIndex][];
                    //these happen in two steps, so that they're all unlocked before signaling occurs
                    for (int rollbackSuccessfulClientsIndex = tryLockClientIndex - 1; rollbackSuccessfulClientsIndex >= 0; rollbackSuccessfulClientsIndex--)
                    {
                        newLockWasCreatedBySuccessfulClient[rollbackSuccessfulClientsIndex] = clients[rollbackSuccessfulClientsIndex].UnlockAll();
                    }

                    clients[tryLockClientIndex].SignalUnlocks(newLockWasCreatedByUnsuccessfulClient);
                    for (int signalSuccessfulClientUnlocksIndex = tryLockClientIndex - 1; signalSuccessfulClientUnlocksIndex >= 0; signalSuccessfulClientUnlocksIndex--)
                    {
                        clients[signalSuccessfulClientUnlocksIndex].SignalUnlocks(newLockWasCreatedBySuccessfulClient[signalSuccessfulClientUnlocksIndex]);
                    }
                    return false;
                }
            }
            keepAliveTimer = new Timer(_ =>
            {
                PreserverLock();
            }, null, keepAliveExpireTime * 1000 / 2, Timeout.Infinite);

            return true;
        }

        void FreeLock()
        {
            lock (syncRoot)
            {
                keepAliveTimer.Dispose();
                foreach (var entry in objectsPaths)
                {
                    entry.Value.UnlockAll();
                }
            }
        }

        /// <summary>
        /// Usefull for operations that depend on a lock to ensure they are
        /// running in the correct thread.
        /// </summary>
        /// <returns></returns>
        public System.Threading.Thread GetRequiredThread() { return requiredThread; }

        public void Dispose()
        {
            signal.Dispose();
            keepAliveTimer.Dispose();
        }
    }
}
