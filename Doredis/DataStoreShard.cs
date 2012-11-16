﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Doredis
{
    class DataStoreShard : IDisposable
    {
        ConcurrentDictionary<Thread, RedisProtocolClient> perThreadClients = new ConcurrentDictionary<Thread, RedisProtocolClient>();
        Queue<RedisProtocolClient> availableClients = new Queue<RedisProtocolClient>();
        RedisProtocolClient subscribeListener;
        ConcurrentDictionary<string /*channel name*/, HashSet<Action<string>>> subscriptions;
        string host;
        int port;

        internal DataStoreShard(string host, int port)
        {
            this.host = host;
            this.port = port;
            subscribeListener = Allocate();
        }

        void ListenerLoop()
        {
            throw new NotImplementedException();
            subscribeListener.WaitForData(true);
            subscribeListener.ReadReply();
        }

        /// <summary>
        /// Get a client for this host that is specific to this thread
        /// Having a single client for each thread solves any anticipated
        /// multi-threading problems.
        /// </summary>
        /// <returns></returns>
        RedisProtocolClient GetThreadClient()
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
                    result = RedisProtocolClient.Create(host, port);
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

        internal void Dispose()
        {
            subscribeListener.Dispose();
            foreach (var entry in perThreadClients)
                entry.Key.Join();
            while (availableClients.Count > 0)
                availableClients.Dequeue().Dispose();
        }

        internal void Subscribe(string name, Action<string> handler)
        {
            bool sendSubscribeMessage = false;
            subscriptions.GetOrAdd(name, (string dontCare) => {
                HashSet<Action<string>> handlerList = new HashSet<Action<string>>();
                handlerList.Add(handler);
                sendSubscribeMessage = true;
                return handlerList;
            });
            if (sendSubscribeMessage)
                subscribeListener.Send("subscribe", name);
        }

        internal void Unsubscribe(string name, Action<string> handler)
        {
            HashSet<Action<string>> handlers;
            if (subscriptions.TryGetValue(name, out handlers))
            {
                lock (handlers)
                {
                    handlers.Remove(handler);
                    if (handlers.Count == 0)
                        subscribeListener.Send("unsubscribe", name);
                }
            }
        }
    }
}
