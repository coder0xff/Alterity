using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Doredis
{
    public class Lock: IDisposable
    {
        const int keepAliveExpireTime = 10; //if the server that held the lock goes down, this'll keep all the other servers from waiting forever
        readonly static string CreateLockScript = "if redis.call('get', KEYS[1]) == false then redis.call('set', KEYS[1], 0); redis.call('expire', KEYS[1], " + keepAliveExpireTime.ToString() + "); return 1; else return 0; end";
        readonly static string CreateLockScriptSha1 = CreateLockScript.Utf8Sha1Hash();
        readonly static string DecrementAndDestroyScript = "if redis.call('get', KEYS[1]) == '1' then redis.call('del', KEYS[1]); return 1; else redis.call('decr', KEYS[1]); return 0; end";
        readonly static string DecrementAndDestroyScriptSha1 = DecrementAndDestroyScript.Utf8Sha1Hash();

        class ClientEntry : IDisposable
        {
            [ThreadStatic]
            static HashSet<string> lockedPaths;

            DataStoreShard shard;
            Action<string> signalHandler;
            public List<IDataObject> dataObjects = new List<IDataObject>();
            /// <summary>
            /// used to make sure that this client entry doesn't react to its own signals
            /// </summary>
            public Guid guid = Guid.NewGuid();
            public ClientEntry(DataStoreShard shard, Action<string> signalHandler)
            {
                this.shard = shard;
                this.signalHandler = signalHandler;
            }

            public void Add(IDataObject dataObject)
            {
                shard.Subscribe(dataObject.GetAbsolutePath(), signalHandler);
                dataObjects.Add(dataObject);
            }

            string GetLockPath(IDataObject dataObject)
            {
                return dataObject.GetAbsolutePath() + ".__lock__";
            }

            bool[] AllIndicesTrue()
            {
                bool[] result = new bool[dataObjects.Count];
                for (int index = 0; index < dataObjects.Count; index++)
                    result[index] = true;
                return result;
            }

            bool ThreadHasLockedIDataObject(IDataObject dataObject)
            {
                if (lockedPaths == null) lockedPaths = new HashSet<string>();
                return lockedPaths.Contains(dataObject.GetAbsolutePath());
            }

            void SetThreadHasLockedIDataObject(IDataObject dataObject)
            {
                if (lockedPaths == null) lockedPaths = new HashSet<string>();
                lockedPaths.Add(dataObject.GetAbsolutePath());
            }

            void ClearThreadHasLockedIDataObject(IDataObject dataObject)
            {
                if (lockedPaths == null) lockedPaths = new HashSet<string>();
                lockedPaths.Remove(dataObject.GetAbsolutePath());
            }

            public bool TryCreateLocks(out bool[] createdLocks)
            {
                bool success = true;
                createdLocks = new bool[dataObjects.Count];

                bool[] createdLocksProxy = createdLocks;
                shard.Transaction(trans =>
                {
                    for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    {
                        IDataObject dataObject = dataObjects[dataObjectIndex];
                        string atomicLockPath = GetLockPath(dataObject);

                        if (!ThreadHasLockedIDataObject(dataObject))
                        {
                            int dataObjectIndexCopy = dataObjectIndex;
                            trans.ExecuteScript(CreateLockScriptSha1, new string[] { GetLockPath(dataObject) }, r => success &= createdLocksProxy[dataObjectIndexCopy] = r.Expect<bool>());
                        }
                    }
                });

                for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    if (createdLocks[dataObjectIndex]) SetThreadHasLockedIDataObject(dataObjects[dataObjectIndex]);

                if (!success) return false;

                foreach (IDataObject dataObject in dataObjects)
                    shard.Unsubscribe(dataObject.GetAbsolutePath(), signalHandler);

                IncrementLockCounts();
                return true;
            }

            void IncrementLockCounts()
            {
                shard.Transaction(trans =>
                {
                    for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    {
                        IDataObject dataObject = dataObjects[dataObjectIndex];
                        string lockPath = GetLockPath(dataObject);
                            trans.Increment(lockPath, r => r.Expect<long>());
                    }                    
                });
            }

            bool[] DecrementAndDestroyLocks()
            {
                bool[] result = new bool[dataObjects.Count];
                shard.Transaction(trans =>
                {
                    for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    {
                        int dataObjectIndexCopy = dataObjectIndex;
                        IDataObject dataObject = dataObjects[dataObjectIndexCopy];
                        string lockPath = GetLockPath(dataObject);
                        trans.ExecuteScript(DecrementAndDestroyScriptSha1, new string[] { lockPath }, _ => result[dataObjectIndexCopy] = _.Expect<bool>());
                    }
                });

                for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    if (result[dataObjectIndex]) ClearThreadHasLockedIDataObject(dataObjects[dataObjectIndex]);

                return result;
            }

            public void DestroyLocks(bool[] markedIndices)
            {
                shard.Transaction(trans =>
                {
                    for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    {
                        if (markedIndices[dataObjectIndex])
                        {
                            IDataObject dataObject = dataObjects[dataObjectIndex];
                            string atomicLockPath = GetLockPath(dataObject);
                            trans.Delete(atomicLockPath, r => r.Expect<OkReply>());
                        }
                    }
                });

                for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    ClearThreadHasLockedIDataObject(dataObjects[dataObjectIndex]);
            }

            public void SignalUnlocks(bool[] markedIndices)
            {
                shard.Transaction(trans => 
                {
                    for (int dataObjectIndex = 0; dataObjectIndex < dataObjects.Count; dataObjectIndex++)
                    {
                        if (markedIndices[dataObjectIndex])
                        {
                            IDataObject dataObject = dataObjects[dataObjectIndex];
                            trans.Publish(dataObject.GetAbsolutePath(), guid.ToString(), r => r.Expect<int>());
                        }
                    }
                });
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
                return DecrementAndDestroyLocks();
            }

            public void Dispose()
            {
                dataObjects.Clear();
            }
        }

        readonly ManualResetEvent signal;
        readonly Dictionary<DataStoreShard, ClientEntry> clientEntries = new Dictionary<DataStoreShard, ClientEntry>();

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
            requiredThread = System.Threading.Thread.CurrentThread;
            syncRoot = 0;

            if (dataObjects.Length == 0)
            {
                succeeded = true;
                hasLock = true;
                isWaiting = false;
            }
            else
            {
                ((IDataObject)dataObjects[0]).GetDataStore().UploadScript(CreateLockScript, CreateLockScriptSha1);
                ((IDataObject)dataObjects[0]).GetDataStore().UploadScript(DecrementAndDestroyScript, DecrementAndDestroyScriptSha1);

                foreach (object dataObject in dataObjects)
                    if (!(dataObject is IDataObject))
                        throw new ArgumentException("All the objects must be IDataObjects created by Doredis");

                succeeded = false;
                hasLock = false;
                isWaiting = true;
                signal = new ManualResetEvent(true);
                Timer timeoutTimer = new Timer(_ => Fail(), null, timeoutMilliseconds, Timeout.Infinite);
                try
                {
                    foreach (IDataObject dataObject in dataObjects)
                    {
                        DataStoreShard dataStore = dataObject.GetDataStoreShard(dataObject.GetAbsolutePath());
                        Action<string> signalHandler = (string dontCare) => { signal.Set(); };
                        if (!clientEntries.ContainsKey(dataStore)) clientEntries[dataStore] = new ClientEntry(dataStore, signalHandler);
                        clientEntries[dataStore].Add(dataObject);
                    }
                    while (true)
                    {
                        try
                        {
                            lock (syncRoot)
                            {
                                signal.Reset();
                                if (isWaiting)
                                {
                                    hasLock = TryLock();
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
                                succeeded = true;
                                action();
                                break;
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
                    foreach (var entry in clientEntries)
                    {
                        entry.Value.Dispose();
                    }
                    timeoutTimer.Dispose();
                    signal.Dispose();
                }
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

        void KeepAlive()
        {
            lock (syncRoot)
            {
                if (hasLock)
                {
                    foreach (var entry in clientEntries)
                    {
                        entry.Value.KeepAliveAll();
                    }
                }
            }
        }

        bool TryLock()
        {
            ClientEntry[] clients = clientEntries.Values.ToArray();
            for (int tryLockClientIndex = 0; tryLockClientIndex < clients.Length; tryLockClientIndex++)
            {
                bool[] newLockWasCreatedByUnsuccessfulClient;
                if (!clients[tryLockClientIndex].TryCreateLocks(out newLockWasCreatedByUnsuccessfulClient))
                {
                    clients[tryLockClientIndex].DestroyLocks(newLockWasCreatedByUnsuccessfulClient);
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
                KeepAlive();
            }, null, keepAliveExpireTime * 1000 / 2, keepAliveExpireTime * 1000 / 2);

            return true;
        }

        void FreeLock()
        {
            lock (syncRoot)
            {
                keepAliveTimer.Dispose();
                Dictionary<ClientEntry, bool[]> pathsToSignal = new Dictionary<ClientEntry, bool[]>();
                foreach (var entry in clientEntries)
                {
                    pathsToSignal[entry.Value] = entry.Value.UnlockAll();
                }
                foreach (var entry in clientEntries)
                {
                    entry.Value.SignalUnlocks(pathsToSignal[entry.Value]);
                }
            }
        }

        /// <summary>
        /// Useful for operations that depend on a lock to ensure they are
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
