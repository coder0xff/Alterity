using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Redis
{
    class Lock
    {
        static int keepAliveExpireTime = 10; //if the server that held the lock goes down, this'll keep all the other servers from waiting forever

        class ClientEntry : IDisposable
        {
            public ServiceStack.Redis.RedisClient client;
            public ServiceStack.Redis.IRedisSubscription subscription;
            public List<string> objectPaths;

            public ClientEntry(ServiceStack.Redis.RedisClient client, Action signalHandler)
            {
                this.client = new ServiceStack.Redis.RedisClient(client.Host); //need a new instance for PreserveAll
                subscription = client.CreateSubscription();
                subscription.OnMessage = (dontCare1, dontCare2) => signalHandler();
                objectPaths = new List<string>();
            }

            public void Add(string absolutePath)
            {
                subscription.SubscribeToChannels(absolutePath);
                objectPaths.Add(absolutePath);
            }

            public bool TryLockAll()
            {
                bool[] successes = new bool[objectPaths.Count];
                using (var trans = client.CreateTransaction())
                {
                    for (int objectPathIndex = 0; objectPathIndex < objectPaths.Count; objectPathIndex++)
                    {
                        string objectPath = objectPaths[objectPathIndex];
                        trans.QueueCommand(c => c.Get<int>(objectPath), r => successes[objectPathIndex] = r == 0);
                        trans.QueueCommand(c => c.Set(objectPath, 1, TimeSpan.FromSeconds(Lock.keepAliveExpireTime)));
                    }

                    trans.Commit();
                }

                return successes.All(_ => _);
            }

            public void PreserverLock()
            {
                using (var trans = client.CreateTransaction())
                {
                    for (int objectPathIndex = 0; objectPathIndex < objectPaths.Count; objectPathIndex++)
                    {
                        string objectPath = objectPaths[objectPathIndex];
                        trans.QueueCommand(c => c.ExpireEntryIn(objectPath, TimeSpan.FromSeconds(Lock.keepAliveExpireTime)));
                    }

                    trans.Commit();
                }
            }

            public void UnlockAll()
            {
                using (var trans = client.CreateTransaction())
                {
                    for (int objectPathIndex = 0; objectPathIndex < objectPaths.Count; objectPathIndex++)
                    {
                        string objectPath = objectPaths[objectPathIndex];
                        trans.QueueCommand(c => c.Set(objectPath, 0));
                    }

                    trans.Commit();
                }
            }

            public void SignalAll()
            {
                using (var trans = client.CreateTransaction())
                {
                    for (int objectPathIndex = 0; objectPathIndex < objectPaths.Count; objectPathIndex++)
                    {
                        string objectPath = objectPaths[objectPathIndex];
                        trans.QueueCommand(c => c.PublishMessage(objectPath, ""));
                    }

                    trans.Commit();
                }
            }

            public void Dispose()
            {
                subscription.UnSubscribeFromAllChannels();
                objectPaths.Clear();
            }
        }

        bool isWaiting;
        Object syncRoot;
        ManualResetEvent signal;
        Dictionary<ServiceStack.Redis.RedisClient, ClientEntry> objectsPaths;
        Timer keepAliveTimer;
        Timer deadlockBreakingTimer; //if the server that held the lock goes down, this'll keep all the other servers from waiting forever
        bool hasLock;
        bool succeeded;

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
            succeeded = false;
            hasLock = false;
            isWaiting = true;
            syncRoot = 0; //just a boxed dummy value
            signal = new ManualResetEvent(true);
            Timer timeoutTimer = new Timer(_ => Fail(), null, timeoutMilliseconds, Timeout.Infinite);
            deadlockBreakingTimer = new Timer(_ => signal.Set(), null, keepAliveExpireTime * 1000, keepAliveExpireTime * 1000);
            try
            {
                foreach (IDataObject dataObject in dataObjects)
                {
                    string absolutePath = dataObject.GetMemberAbsolutePath("__lock__", false);
                    ServiceStack.Redis.RedisClient dataStore = dataObject.GetDataStore(absolutePath);
                    if (!objectsPaths.ContainsKey(dataStore)) objectsPaths[dataStore] = new ClientEntry(dataStore, () => signal.Set());
                    objectsPaths[dataStore].Add(absolutePath);
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
                                    timeoutTimer.Dispose();
                                }
                            }
                            else
                                break;
                        }
                        if (hasLock)
                        {
                            succeeded = true;
                            action();
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
                        entry.Value.PreserverLock();
                    }
                }
            }
        }

        bool TryAttainLock()
        {
            ClientEntry[] clients = objectsPaths.Values.ToArray();
            bool[] successes = new bool[objectsPaths.Count];
            for (int clientIndex = 0; clientIndex < clients.Length; clientIndex++)
            {
                if (clients[clientIndex].TryLockAll())
                {
                    successes[clientIndex] = true;
                }
                else
                {
                    for (int unlockClientIndex = clientIndex - 1; unlockClientIndex >= 0; unlockClientIndex--)
                    {
                        clients[unlockClientIndex].UnlockAll();
                    }
                    for (int signalClientIndex = clientIndex - 1; signalClientIndex >= 0; signalClientIndex--)
                    {
                        clients[signalClientIndex].SignalAll();
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
    }
}
