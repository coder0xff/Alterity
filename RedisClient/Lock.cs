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
        bool isWaiting;
        Object syncRoot;
        ManualResetEvent signal;
        ServiceStack.Redis.RedisClient dataStore;
        string absolutePath;
        
        Lock(IDataObject dataObject, int timeoutInMilliseconds, Action action)
        {
            isWaiting = true;
            syncRoot = 0; //just a boxed dummy value
            absolutePath = dataObject.GetMemberAbsolutePath("__lock__", false);
            dataStore = dataObject.GetDataStore(absolutePath);
            ServiceStack.Redis.IRedisSubscription subscription = dataStore.CreateSubscription();
            signal = new ManualResetEvent(true);
            Timer timeoutTimer = new Timer(_ => Fail(), null, timeoutInMilliseconds, Timeout.Infinite);
            try
            {
                subscription.OnMessage = (dontCare1, dontCare2) =>
                {
                    signal.Set();
                };
                syncRoot = 0;
                while (true)
                {
                    lock (syncRoot)
                    {
                        signal.Reset();
                        if (isWaiting)
                        {
                            if (TryAttainLock())
                            {
                                isWaiting = false;
                                try
                                {
                                    action();
                                }
                                finally
                                {
                                    subscription.UnSubscribeFromAllChannels();
                                    timeoutTimer.Dispose();
                                    FreeLock();
                                }
                            }
                        }
                        else
                            break;
                    }
                    signal.WaitOne();
                }
            }
            finally
            {
                subscription.UnSubscribeFromAllChannels();
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

        bool TryAttainLock()
        {
            byte[] atomicResult = dataStore.GetSet(absolutePath, new byte[] { 1 });
            if (atomicResult == null || atomicResult.Length == 0 || atomicResult[0] == 0)
                return true;
            else
                return false;
        }

        void FreeLock()
        {
            dataStore.Set(absolutePath, new byte[] { 0 });
            dataStore.Publish(absolutePath, new byte[] { 0 });
        }
    }
}
