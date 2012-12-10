using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;

namespace Doredis
{
    class DataStoreShard : IStructuredDataClient, IDisposable
    {
        ConcurrentDictionary<Thread, RedisProtocolClient> perThreadClients = new ConcurrentDictionary<Thread, RedisProtocolClient>();
        Queue<RedisProtocolClient> availableClients = new Queue<RedisProtocolClient>();
        RedisProtocolClient subscribeListenerClient;
        Thread subscribeListenerThread;
        Dictionary<string, HashSet<Action<string>>> subscriptions = new Dictionary<string, HashSet<Action<string>>>();
        Queue<Action> pendingSubscriptionModifications = new Queue<Action>();
        ManualResetEvent pendingSubscriptionModificationsAdded = new ManualResetEvent(false);
        System.Net.HostEndPoint endPoint;
        DataStore owner;

        internal DataStoreShard(DataStore owner, System.Net.HostEndPoint endPoint)
        {
            this.owner = owner;
            this.endPoint = endPoint;
            subscribeListenerClient = Allocate();
            subscribeListenerThread = new Thread(() => ListenerLoop());
            subscribeListenerThread.Start();
        }

        void ListenerLoop()
        {
            while (true)
            {
                if (pendingSubscriptionModifications.Count == 0 || !subscribeListenerClient.DataIsReady())
                    WaitHandle.WaitAny(new WaitHandle[] { pendingSubscriptionModificationsAdded, subscribeListenerClient.replyStreamBlockReady}, 1000);
                lock (subscriptions)
                {
                    while (pendingSubscriptionModifications.Count > 0)
                        pendingSubscriptionModifications.Dequeue()();
                    pendingSubscriptionModificationsAdded.Reset();
                }
                try
                {
                    if (subscribeListenerClient.DataIsReady())
                    {
                        RedisReply[] reply = (RedisReply[])subscribeListenerClient.ReadReply().Data;
                        string replyType = reply[0].Expect<string>();
                        if (replyType == "message")
                        {
                            string channelName = reply[1].Expect<string>();
                            string message = reply[2].Expect<string>();
                            lock (subscriptions)
                            {
                                HashSet<Action<string>> channelListeners;
                                if (subscriptions.TryGetValue(channelName, out channelListeners))
                                {
                                    foreach (Action<string> channelListener in channelListeners)
                                        channelListener(message);
                                }
                            }
                        }
                    }
                }
                catch (ReplyFormatException)
                {

                }
            }
        }

        /// <summary>
        /// Get a client for this host that is specific to this thread
        /// Having a single client for each thread solves any anticipated
        /// multi-threading problems.
        /// </summary>
        /// <returns></returns>
        internal RedisProtocolClient GetThreadClient()
        {
            return perThreadClients.GetOrAdd(Thread.CurrentThread, (Thread thread) =>
            {
                RedisProtocolClient client = Allocate();
                //When the thread stops, the client will be returned to the pool
                thread.AddStopCallback(() =>
                {
                    Release(client);
                    perThreadClients.TryRemove(thread, out client);
                });
                return client;
            });
        }

        RedisProtocolClient Allocate()
        {
            RedisProtocolClient result;
            lock (availableClients)
            {
                if (availableClients.Count > 0)
                {
                    return availableClients.Dequeue();
                }
            }
            for (int attempts = 0; attempts < 5; attempts++)
            {
                try
                {
                    result = RedisProtocolClient.Create(endPoint);
                    return result;
                }
                catch (Doredis.FailedToConnectException)
                {
                    if (attempts == 4)
                        throw;
                }
            }
            throw new Doredis.FailedToConnectException();
        }

        void Release(RedisProtocolClient client)
        {
            lock (availableClients)
            {
                availableClients.Enqueue(client);
            }
        }

        public void Dispose()
        {
            subscribeListenerClient.Dispose();
            foreach (var entry in perThreadClients)
                entry.Key.Join();
            while (availableClients.Count > 0)
                availableClients.Dequeue().Dispose();
        }

        public void SendRaw(byte[] data)
        {
            GetThreadClient().SendRaw(data);
        }

        internal void Send(params object[] arguments)
        {
            GetThreadClient().SendPackedObjects(arguments);
        }

        public RedisReply ReadReply()
        {
            return GetThreadClient().ReadReply();
        }

        internal void Subscribe(string channelName, Action<string> handler)
        {
            lock (subscriptions)
            {
                pendingSubscriptionModifications.Enqueue(() =>
                {
                    bool sendSubscribeCommand = false;
                    if (!subscriptions.ContainsKey(channelName))
                    {
                        subscriptions[channelName] = new HashSet<Action<string>>();
                        sendSubscribeCommand = true;
                    }
                    HashSet<Action<string>> handlerSet = subscriptions[channelName];
                    handlerSet.Add(handler);
                    if (sendSubscribeCommand)
                        subscribeListenerClient.SendCommandWithPackedObjects("subscribe", new object[] { channelName });
                });
                pendingSubscriptionModificationsAdded.Set();
            }
        }

        internal void Unsubscribe(string channelName, Action<string> handler)
        {
            lock (subscriptions)
            {
                pendingSubscriptionModifications.Enqueue(() =>
                {
                    if (subscriptions.ContainsKey(channelName))
                    {
                        HashSet<Action<string>> handlerSet = subscriptions[channelName];
                        handlerSet.Remove(handler);
                        if (handlerSet.Count == 0)
                        {
                            subscriptions.Remove(channelName);
                            subscribeListenerClient.SendCommandWithPackedObjects("unsubscribe", new object[] { channelName });
                        }
                    }
                });
                pendingSubscriptionModificationsAdded.Set();
            }
        }

        public void SendCommandWithPackedObjects(string command, object[] arguments)
        {
            GetThreadClient().SendCommandWithPackedObjects(command, arguments);
        }

        T Command<T>(string command, object[] arguments)
        {
            T result = default(T);
            CommandWithPackedParameters(command, arguments, _ => result = _.Expect<T>());
            return result;
        }

        public void CommandWithPackedParameters(string command, object[] arguments, Action<RedisReply> resultHandler)
        {
            GetThreadClient().CommandWithPackedParameters(command, arguments, resultHandler);
        }

        public void UploadScript(string scriptSource, string sha1 = null)
        {
            owner.UploadScript(scriptSource, sha1);
        }

        public System.Net.HostEndPoint EndPoint
        {
            get { return endPoint; }
        }

        public DataStore Owner { get { return owner; } }
    }
}
