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
        RedisProtocolClient subscribeListener;
        Dictionary<string, HashSet<Action<string>>> subscriptions = new Dictionary<string, HashSet<Action<string>>>();
        Queue<Action> pendingSubscriptionModifications = new Queue<Action>();
        System.Net.HostEndPoint endPoint;
        HashSet<string> cachedScriptSHA1s = new HashSet<string>();

        internal DataStoreShard(System.Net.HostEndPoint endPoint)
        {
            this.endPoint = endPoint;
            subscribeListener = Allocate();
        }

        void ListenerLoop()
        {
            while (true)
            {
                lock (subscriptions)
                {
                    while (pendingSubscriptionModifications.Count > 0)
                        pendingSubscriptionModifications.Dequeue()();
                }
                try
                {
                    subscribeListener.WaitForData(true);
                    RedisReply[] reply = (RedisReply[])subscribeListener.ReadReply().Data;
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
                    result = RedisProtocolClient.Create(endPoint.Host, endPoint.Port);
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
            subscribeListener.Dispose();
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
                        subscribeListener.SendCommandWithPackedObjects("subscribe", new object[] { channelName });
                });
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
                            subscribeListener.SendCommandWithPackedObjects("unsubscribe", new object[] { channelName });
                        }
                    }
                });
            }
        }

        public void SendCommandWithPackedObjects(string command, object[] arguments)
        {
            GetThreadClient().SendCommandWithPackedObjects(command, arguments);
        }

        T Command<T>(string command, object[] arguments)
        {
            T result = default(T);
            Command(command, arguments, _ => result = _.Expect<T>());
            return result;
        }

        public DelegateType CreateScript<DelegateType>(string scriptText)
        {
            string scriptSha1 = scriptText.Utf8Sha1Hash();
            lock (cachedScriptSHA1s)
            {
                if (!cachedScriptSHA1s.Contains(scriptSha1))
                    cachedScriptSHA1s.Add(this.Command<string>("script", "load", scriptText));
            }

            MethodInfo delegateInfo = DelegateInfo<DelegateType>();
            ParameterInfo[] parameterInfos = delegateInfo.GetParameters();
            int keyCount = parameterInfos.Count(_ => _.ParameterType == typeof(IDataObject));
            Type returnType = delegateInfo.ReturnType;
            if (returnType == typeof(void))
            {
                MethodInfo Command_Method = GetType().GetMethod("Command", new Type[] { typeof(string), typeof(object[]), typeof(Action<RedisReply>) });
                Action<RedisReply> nullReplyExpector = (RedisReply dontCare) => { };
                List<ParameterExpression> incomingParameters = new List<ParameterExpression>();
                Expression evalshaParameterPackExpression = ParameterPackExpression(scriptSha1, parameterInfos, incomingParameters);
                Expression<DelegateType> lambda = Expression.Lambda<DelegateType>(
                    Expression.Call(Expression.Constant(this, typeof(DataStoreShard)), Command_Method, Expression.Constant("evalsha"), evalshaParameterPackExpression, Expression.Constant(nullReplyExpector)),
                    incomingParameters);
                return lambda.Compile();
            }
            else
            {
                MethodInfo Command_Method = GetType().GetMethod("Command", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(returnType);
                List<ParameterExpression> incomingParameters = new List<ParameterExpression>();
                Expression evalshaParameterPackExpression = ParameterPackExpression(scriptSha1, parameterInfos, incomingParameters);
                Expression<DelegateType> lambda = Expression.Lambda<DelegateType>(
                    Expression.Call(Expression.Constant(this, typeof(DataStoreShard)), Command_Method, Expression.Constant("evalsha"), evalshaParameterPackExpression),
                    incomingParameters);
                return lambda.Compile();
            }
        }

        private static MethodInfo DelegateInfo<DelegateType>()
        {
            return typeof(DelegateType).GetMethod("Invoke");
        }

        private static Expression ParameterPackExpression(string scriptSha1, ParameterInfo[] parameterInfos, List<ParameterExpression> incomingParameters)
        {
            Expression parameterPackExpression;
            MethodInfo IPathObject_GetAbsolutePath_Method = typeof(IPathObject).GetMethod("GetAbsolutePath");
            List<Expression> keyParameters = new List<Expression>();
            List<Expression> passThroughParameters = new List<Expression>();
            foreach (System.Reflection.ParameterInfo parameterInfo in parameterInfos)
            {
                ParameterExpression incomingParameter = Expression.Parameter(parameterInfo.ParameterType);
                incomingParameters.Add(incomingParameter);
                if (parameterInfo.ParameterType == typeof(IDataObject))
                {
                    keyParameters.Add(Expression.Convert(Expression.Call(Expression.Convert(incomingParameter, typeof(IPathObject)), IPathObject_GetAbsolutePath_Method), typeof(Object)));
                }
                else
                {
                    passThroughParameters.Add(Expression.Convert(incomingParameter, typeof(Object)));
                }
            }
            List<Expression> allOutgoingParameters = new List<Expression>();
            allOutgoingParameters.Add(Expression.Constant(scriptSha1, typeof(Object)));
            allOutgoingParameters.Add(Expression.Constant(keyParameters.Count, typeof(Object)));
            allOutgoingParameters.AddRange(keyParameters);
            allOutgoingParameters.AddRange(passThroughParameters);
            parameterPackExpression = Expression.NewArrayInit(typeof(Object), allOutgoingParameters.ToArray());
            return parameterPackExpression;
        }

        public void Command(string command, object[] arguments, Action<RedisReply> resultHandler)
        {
            GetThreadClient().Command(command, arguments, resultHandler);
        }
    }
}
