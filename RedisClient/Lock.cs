using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Redis
{
    internal interface ILockable
    {
        void SetLock(Lock lockObject);
        void ClearLock();
        Lock GetLock();
    }

    public class Lock
    {
        static int keepAliveExpireTime = 10; //if the server that held the lock goes down, this'll keep all the other servers from waiting forever

        class ClientEntry : IDisposable
        {
            public ServiceStack.Redis.RedisClient clientCopy;
            public ServiceStack.Redis.RedisClient listenClient;
            public ServiceStack.Redis.IRedisSubscription subscription;
            public List<ILockableDataObject> dataObjects = new List<ILockableDataObject>();
            /// <summary>
            /// used to make sure that this client entry doesn't react to its own signals
            /// </summary>
            public Guid guid = Guid.NewGuid();
            public ClientEntry(ServiceStack.Redis.RedisClient client, Action signalHandler)
            {
                this.clientCopy = new ServiceStack.Redis.RedisClient(client.Host); //need a new instance for KeepAliveAll
                listenClient = new ServiceStack.Redis.RedisClient(client.Host); //need a new instance to listen on
                subscription = listenClient.CreateSubscription();
                subscription.OnMessage = (dontCare, clientEntryGuid) => { if (clientEntryGuid != guid.ToString()) signalHandler(); };
            }

            public void Add(ILockableDataObject dataObject)
            {
                subscription.SubscribeToChannels(dataObject.GetAbsolutePath());
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
                using (var trans = clientCopy.CreateTransaction())
                {
                    for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    {
                        ILockableDataObject dataObject = dataObjects[dataObjectIndex];
                        string lockCountPath = GetLockCountPath(dataObject);
                        if (forceToOneIndices[dataObjectIndex])
                            trans.QueueCommand(c => c.Set(lockCountPath, 1));
                        else
                            trans.QueueCommand(c => c.IncrementValue(lockCountPath));
                    }

                    trans.Commit();
                }
            }

            bool[] DecrementLockCounts()
            {
                bool[] result = new bool[dataObjects.Count];
                using (var trans = clientCopy.CreateTransaction())
                {
                    for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    {
                        ILockableDataObject dataObject = dataObjects[dataObjectIndex];
                        string lockCountPath = GetLockCountPath(dataObject);
                        trans.QueueCommand(c => c.DecrementValue(lockCountPath), r => result[dataObjectIndex] = r == 0);
                    }

                    trans.Commit();
                }

                return result;
            }

            public void ClearLocks(bool[] markedIndices)
            {
                using (var trans = clientCopy.CreateTransaction())
                {
                    for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    {
                        if (markedIndices[dataObjectIndex])
                        {
                            ILockableDataObject dataObject = dataObjects[dataObjectIndex];
                            string atomicLockPath = GetLockPath(dataObject);
                            trans.QueueCommand(c => c.Set(atomicLockPath, 0));
                        }
                    }
                    trans.Commit();
                }
            }

            public void SignalUnlocks(bool[] markedIndices)
            {
                using (var trans = clientCopy.CreateTransaction())
                {
                    for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    {
                        if (markedIndices[dataObjectIndex])
                        {
                            ILockableDataObject dataObject = dataObjects[dataObjectIndex];
                            trans.QueueCommand(c => c.PublishMessage(dataObject.GetAbsolutePath(), guid.ToString()));
                        }
                    }
                    trans.Commit();
                }
            }

            public bool TryLockAll(out bool[] neededUnlocks)
            {
                bool[] successes = new bool[dataObjects.Count];
                // make sure we reset the count on new locks, in case a lock was abandoned
                bool[] isNewLock = new bool[dataObjects.Count];

                using (var trans = clientCopy.CreateTransaction())
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
                                trans.Rollback();
                                neededUnlocks = new bool[dataObjects.Count];
                                return false;
                            }
                        }
                        else
                        {
                            trans.QueueCommand(c => c.Get<int>(atomicLockPath), r => successes[dataObjectIndex] = isNewLock[dataObjectIndex] = r == 0);
                            trans.QueueCommand(c => c.Set(atomicLockPath, 1, TimeSpan.FromSeconds(Lock.keepAliveExpireTime)));
                        }
                    }

                    trans.Commit();
                }
                
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
                    subscription.UnSubscribeFromAllChannels();
                    listenClient.Dispose();
                    IncrementLockCounts(isNewLock);
                    return true;
                }
            }

            public void KeepAliveAll()
            {
                using (var trans = clientCopy.CreateTransaction())
                {
                    for (int objectPathIndex = 0; objectPathIndex < dataObjects.Count; objectPathIndex++)
                    {
                        string objectPath = GetLockPath(dataObjects[objectPathIndex]);
                        trans.QueueCommand(c => c.ExpireEntryIn(objectPath, TimeSpan.FromSeconds(Lock.keepAliveExpireTime)));
                    }

                    trans.Commit();
                }
            }

            public bool[] UnlockAll()
            {
                bool[] trulyUnlocked = DecrementLockCounts();
                ClearLocks(trulyUnlocked);
                return trulyUnlocked;
            }

            public void Dispose()
            {
                clientCopy.Dispose();
                listenClient.Dispose();
                dataObjects.Clear();
            }
        }

        readonly ManualResetEvent signal;
        readonly Dictionary<ServiceStack.Redis.RedisClient, ClientEntry> objectsPaths = new Dictionary<ServiceStack.Redis.RedisClient, ClientEntry>();
        /// <summary>
        /// If the server that held the lock goes down, this'll keep all the other servers from waiting forever.
        /// </summary>
        readonly Timer deadlockBreakingTimer;
        readonly Thread requiredThread;
        bool isWaiting;
        Object syncRoot;
        bool hasLock;
        bool succeeded;
        Timer keepAliveTimer;

        public static bool On(IDataObject dataObject, Action action)
        {
            return On(dataObject, Timeout.Infinite, action);
        }

        public static bool On(IDataObject dataObject, int timeoutMilliseconds, Action action)
        {
            return On(new IDataObject[] { dataObject }, timeoutMilliseconds, action);
        }

        public static bool On(IDataObject[] dataObjects, int timeoutMilliseconds, Action action)
        {
            return (new Lock(dataObjects, timeoutMilliseconds, action)).succeeded;
        }

        Lock(IDataObject[] dataObjects, int timeoutMilliseconds, Action action)
        {
            requiredThread = System.Threading.Thread.CurrentThread;
            succeeded = false;
            hasLock = false;
            isWaiting = true;
            syncRoot = 0; //just a boxed dummy value
            signal = new ManualResetEvent(true);
            Timer timeoutTimer = new Timer(_ => Fail(), null, timeoutMilliseconds, Timeout.Infinite);
            deadlockBreakingTimer = new Timer(_ => signal.Set(), null, keepAliveExpireTime * 1000, keepAliveExpireTime * 1000);
            try
            {
                foreach (ILockableDataObject dataObject in dataObjects)
                {
                    ServiceStack.Redis.RedisClient dataStore = dataObject.GetDataStore(dataObject.GetAbsolutePath());
                    if (!objectsPaths.ContainsKey(dataStore)) objectsPaths[dataStore] = new ClientEntry(dataStore, () => signal.Set());
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
                                deadlockBreakingTimer.Change(keepAliveExpireTime * 1000, keepAliveExpireTime * 1000);
                                hasLock = TryAttainLock();
                                if (hasLock)
                                {
                                    deadlockBreakingTimer.Dispose();
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
                    signal.WaitOne();
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
                deadlockBreakingTimer.Dispose();
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
    }
}
