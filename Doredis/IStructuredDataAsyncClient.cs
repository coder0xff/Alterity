using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doredis
{
    internal interface IStructuredDataAsyncClient
    {
        void CommandWithPackedParameters(string command, object[] arguments, Action<RedisReply> resultHandler = null);
        System.Net.HostEndPoint EndPoint { get; }
    }

    static class IStructuredDataAsyncClientExtensions
    {
        internal static void Decrement(this IStructuredDataAsyncClient self, string keyName, Action<RedisReply> resultHandler = null)
        {
            self.CommandWithPackedParameters("DECR", new object[] { keyName }, resultHandler);
        }

        internal static void Exists(this IStructuredDataAsyncClient self, string keyName, Action<RedisReply> resultHandler = null)
        {
            self.CommandWithPackedParameters("EXISTS", new object[] { keyName }, resultHandler);
        }

        internal static void Expire(this IStructuredDataAsyncClient self, string keyName, int durationInSeconds, Action<RedisReply> resultHandler = null)
        {
            self.CommandWithPackedParameters("EXPIRE", new object[] { keyName, durationInSeconds.ToString() }, resultHandler);
        }

        internal static void Get(this IStructuredDataAsyncClient self, string keyName, Action<RedisReply> resultHandler = null)
        {
            self.CommandWithPackedParameters("GET", new object[] { keyName }, resultHandler);
        }

        internal static void Increment(this IStructuredDataAsyncClient self, string keyName, Action<RedisReply> resultHandler = null)
        {
            self.CommandWithPackedParameters("INCR", new object[] { keyName }, resultHandler);
        }

        internal static void Publish(this IStructuredDataAsyncClient self, string channelName, string message, Action<RedisReply> resultHandler = null)
        {
            self.CommandWithPackedParameters("PUBLISH", new object[] { channelName, message }, resultHandler);
        }

        internal static void Set(this IStructuredDataAsyncClient self, string keyName, object value, Action<RedisReply> resultHandler = null)
        {
            self.CommandWithPackedParameters("SET", new object[] { keyName, value }, resultHandler);
        }

        internal static void Delete(this IStructuredDataAsyncClient self, string keyName, Action<RedisReply> resultHandler = null)
        {
            self.CommandWithPackedParameters("DEL", new object[] { keyName }, resultHandler);
        }

        internal static void ExecuteScript(this IStructuredDataAsyncClient self, string scriptSha1, IDataObject[] keys, Action<RedisReply> resultHandler = null)
        {
            ExecuteScript(self, scriptSha1, keys.Select(_ => _.GetAbsolutePath()).ToArray(), resultHandler);
        }

        internal static void ExecuteScript(this IStructuredDataAsyncClient self, string scriptSha1, string[] keys, Action<RedisReply> resultHandler = null)
        {
            ExecuteScript(self, scriptSha1, keys, new object[0], resultHandler);
        }

        internal static void ExecuteScript(this IStructuredDataAsyncClient self, string scriptSha1, IDataObject[] keys, Object[] arguments, Action<RedisReply> resultHandler = null)
        {
            ExecuteScript(self, scriptSha1, keys.Select(_ => _.GetAbsolutePath()).ToArray(), arguments, resultHandler);
        }

        internal static void ExecuteScript(this IStructuredDataAsyncClient self, string scriptSha1, string[] keys, Object[] arguments, Action<RedisReply> resultHandler = null)
        {
            List<object> parameters = new List<object>();
            parameters.Add(scriptSha1);
            parameters.Add(keys.Length);
            parameters.AddRange(keys);
            parameters.AddRange(arguments);
            self.CommandWithPackedParameters("EVALSHA", parameters.ToArray(), resultHandler);
        }
    }
}
