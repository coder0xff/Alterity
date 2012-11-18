using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doredis
{
    interface IStructuredDataClient : IStructuredDataAsyncClient
    {
        void SendRaw(byte[] data);
        RedisReply ReadReply();
    }

    static class IStructuredDataClientExtensions
    {
        internal static T Command<T>(this IStructuredDataClient self, string command, params object[] arguments)
        {
            T result = default(T);
            self.Command(command, arguments, _ => result = _.Expect<T>());
            return result;
        }

        internal static long Decrement(this IStructuredDataClient self, string keyName)
        {
            return self.Command<long>("DECR", keyName);
        }

        internal static void Discard(this IStructuredDataClient self)
        {
            self.Command<OkReply>("DISCARD");
        }

        internal static RedisReply[] Exec(this IStructuredDataClient self)
        {
            return self.Command<RedisReply[]>("EXEC");
        }

        internal static bool Exists(this IStructuredDataClient self, string keyName)
        {
            return self.Command<bool>("EXITS", keyName);
        }

        internal static void Expire(this IStructuredDataClient self, string keyName, int durationInSeconds)
        {
            self.Command<OkReply>("EXPIRE", keyName, durationInSeconds.ToString());
        }

        internal static RedisReply Get(this IStructuredDataClient self, string keyName)
        {
            return self.Command<RedisReply>("GET", keyName);
        }

        internal static T Get<T>(this IStructuredDataClient self, string keyName)
        {
            return Get(self, keyName).Expect<T>();
        }

        internal static long Increment(this IStructuredDataClient self, string keyName)
        {
            return self.Command<long>("INCR", keyName);
        }

        internal static void Multi(this IStructuredDataClient self)
        {
            self.Command<OkReply>("MULTI");
        }

        internal static void Publish(this IStructuredDataClient self, string keyName, object value)
        {
            self.Command<OkReply>("PUBLISH", keyName, value);
        }

        internal static void Set(this IStructuredDataClient self, string keyName, object value)
        {
            self.Command<OkReply>("SET", keyName, value);
        }

        internal static void Transaction(this IStructuredDataClient self, Action<Transaction.ITransactionClient> transactionBuilder)
        {
            try
            {
                new Transaction(self, transactionBuilder).Execute();
            }
            catch (Transaction.TransactionCancelledException)
            {
            	
            }
        }

        internal static void Watch(this IStructuredDataClient self, string[] keyNames)
        {
            if (keyNames.Length > 0)
                self.Command("WATCH", keyNames, _ => _.Expect<OkReply>());
        }

    }
}
