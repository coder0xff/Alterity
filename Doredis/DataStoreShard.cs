using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Doredis
{
    class DataStoreShard : IStructuredDataClient, IDisposable
    {
        readonly ConcurrentDictionary<Thread, RedisProtocolClient> _perThreadClients = new ConcurrentDictionary<Thread, RedisProtocolClient>();
        readonly Queue<RedisProtocolClient> _availableClients = new Queue<RedisProtocolClient>();
        readonly RedisProtocolClient _subscribeListenerClient;
        readonly Thread _subscribeListenerThread;
        readonly Dictionary<string, HashSet<Action<string>>> _subscriptions = new Dictionary<string, HashSet<Action<string>>>();
        readonly Queue<Action> _pendingSubscriptionModifications = new Queue<Action>();
        readonly ManualResetEvent _pendingSubscriptionModificationsAdded = new ManualResetEvent(false);
        readonly System.Net.HostEndPoint _endPoint;
        readonly DataStore _owner;

        internal DataStoreShard(DataStore owner, System.Net.HostEndPoint endPoint)
        {
            _owner = owner;
            _endPoint = endPoint;
            _subscribeListenerClient = Allocate();
            _subscribeListenerThread = new Thread(ListenerLoop);
            _subscribeListenerThread.Start();
        }

        void ListenerLoop()
        {
            while (true)
            {
                if (_pendingSubscriptionModifications.Count == 0 || !_subscribeListenerClient.DataIsReady())
                    WaitHandle.WaitAny(new WaitHandle[] { _pendingSubscriptionModificationsAdded, _subscribeListenerClient.ReplyStreamBlockReady}, 1000);
                lock (_subscriptions)
                {
                    while (_pendingSubscriptionModifications.Count > 0)
                        _pendingSubscriptionModifications.Dequeue()();
                    _pendingSubscriptionModificationsAdded.Reset();
                }
                try
                {
                    if (_subscribeListenerClient.DataIsReady())
                    {
                        var reply = (RedisReply[])_subscribeListenerClient.ReadReply().Data;
                        var replyType = reply[0].Expect<string>();
                        if (replyType == "message")
                        {
                            var channelName = reply[1].Expect<string>();
                            var message = reply[2].Expect<string>();
                            lock (_subscriptions)
                            {
                                HashSet<Action<string>> channelListeners;
                                if (_subscriptions.TryGetValue(channelName, out channelListeners))
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
            return _perThreadClients.GetOrAdd(Thread.CurrentThread, thread =>
            {
                RedisProtocolClient client = Allocate();
                //When the thread stops, the client will be returned to the pool
                thread.AddStopCallback(() =>
                {
                    Release(client);
                    _perThreadClients.TryRemove(thread, out client);
                });
                return client;
            });
        }

        RedisProtocolClient Allocate()
        {
            lock (_availableClients)
            {
                if (_availableClients.Count > 0)
                {
                    return _availableClients.Dequeue();
                }
            }
            for (var attempts = 0; attempts < 5; attempts++)
            {
                try
                {
                    RedisProtocolClient result = RedisProtocolClient.Create(_endPoint);
                    return result;
                }
                catch (FailedToConnectException)
                {
                    if (attempts == 4)
                        throw;
                }
            }
            throw new FailedToConnectException();
        }

        void Release(RedisProtocolClient client)
        {
            lock (_availableClients)
            {
                _availableClients.Enqueue(client);
            }
        }

        public void Dispose()
        {
            _subscribeListenerClient.Dispose();
            foreach (var entry in _perThreadClients)
                entry.Key.Join();
            while (_availableClients.Count > 0)
                _availableClients.Dequeue().Dispose();
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
            lock (_subscriptions)
            {
                _pendingSubscriptionModifications.Enqueue(() =>
                {
                    bool sendSubscribeCommand = false;
                    if (!_subscriptions.ContainsKey(channelName))
                    {
                        _subscriptions[channelName] = new HashSet<Action<string>>();
                        sendSubscribeCommand = true;
                    }
                    HashSet<Action<string>> handlerSet = _subscriptions[channelName];
                    handlerSet.Add(handler);
                    if (sendSubscribeCommand)
                        _subscribeListenerClient.SendCommandWithPackedObjects("subscribe", new object[] { channelName });
                });
                _pendingSubscriptionModificationsAdded.Set();
            }
        }

        internal void Unsubscribe(string channelName, Action<string> handler)
        {
            lock (_subscriptions)
            {
                _pendingSubscriptionModifications.Enqueue(() =>
                {
                    if (_subscriptions.ContainsKey(channelName))
                    {
                        HashSet<Action<string>> handlerSet = _subscriptions[channelName];
                        handlerSet.Remove(handler);
                        if (handlerSet.Count == 0)
                        {
                            _subscriptions.Remove(channelName);
                            _subscribeListenerClient.SendCommandWithPackedObjects("unsubscribe", new object[] { channelName });
                        }
                    }
                });
                _pendingSubscriptionModificationsAdded.Set();
            }
        }

        public void SendCommandWithPackedObjects(string command, object[] arguments)
        {
            GetThreadClient().SendCommandWithPackedObjects(command, arguments);
        }

        public void CommandWithPackedParameters(string command, object[] arguments, Action<RedisReply> resultHandler)
        {
            GetThreadClient().CommandWithPackedParameters(command, arguments, resultHandler);
        }

        public void UploadScript(string scriptSource, string sha1 = null)
        {
            _owner.UploadScript(scriptSource, sha1);
        }

        public System.Net.HostEndPoint EndPoint
        {
            get { return _endPoint; }
        }

        public DataStore Owner { get { return _owner; } }
    }
}
