using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doredis
{
    internal interface IStructuredDataAsyncClient
    {
        void Command(string command, object[] arguments, Action<RedisReply> resultHandler = null);
        DelegateType CreateScript<DelegateType>(string scriptText);
        System.Net.HostEndPoint EndPoint { get; }
    }

    static class IStructuredDataCommandSenderExtensions
    {
        public static void Decrement(this IStructuredDataAsyncClient self, string keyName, Action<RedisReply> resultHandler = null)
        {
            self.Command("DECR", new object[] { keyName }, resultHandler);
        }

        public static void Exists(this IStructuredDataAsyncClient self, string keyName, Action<RedisReply> resultHandler = null)
        {
            self.Command("EXISTS", new object[] { keyName }, resultHandler);
        }

        public static void Expire(this IStructuredDataAsyncClient self, string keyName, int durationInSeconds, Action<RedisReply> resultHandler = null)
        {
            self.Command("EXPIRE", new object[] { keyName, durationInSeconds.ToString() }, resultHandler);
        }

        public static void Get(this IStructuredDataAsyncClient self, string keyName, Action<RedisReply> resultHandler = null)
        {
            self.Command("GET", new object[] { keyName }, resultHandler);
        }

        public static void Increment(this IStructuredDataAsyncClient self, string keyName, Action<RedisReply> resultHandler = null)
        {
            self.Command("INCR", new object[] { keyName }, resultHandler);
        }

        public static void Publish(this IStructuredDataAsyncClient self, string channelName, string message, Action<RedisReply> resultHandler = null)
        {
            self.Command("PUBLISH", new object[] { channelName, message }, resultHandler);
        }

        public static void Set(this IStructuredDataAsyncClient self, string keyName, object value, Action<RedisReply> resultHandler = null)
        {
            self.Command("SET", new object[] { keyName, value }, resultHandler);
        }

    }
}
