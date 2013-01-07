using System;
using System.Collections.Generic;
using System.Globalization;

namespace Doredis
{
    interface IStructuredDataClient : IStructuredDataAsyncClient
    {
        void SendRaw(byte[] data);
        RedisReply ReadReply();
    }

    static class StructuredDataClientExtensions
    {
        internal static T Command<T>(this IStructuredDataClient self, string command, params object[] parameters)
        {
            T result = default(T);
            self.CommandWithPackedParameters(command, parameters, _ => result = _.Expect<T>());
            return result;
        }

        internal static T CommandWithPackedParameters<T>(this IStructuredDataClient self, string command, object[] parameters)
        {
            T result = default(T);
            self.CommandWithPackedParameters(command, parameters, _ => result = _.Expect<T>());
            return result;
        }

        internal static Object CommandWithPackedParameters(this IStructuredDataClient self, Type expectedType, string command, object[] parameters)
        {
            Object result = null;
            self.CommandWithPackedParameters(command, parameters, _ => result = _.Expect(expectedType));
            return result;
        }

        internal static Object Command(this IStructuredDataClient self, Type expectedType, string command, params object[] parameters)
        {
            return CommandWithPackedParameters(self, expectedType, command, parameters);
        }

        internal static long Decrement(this IStructuredDataClient self, string keyName)
        {
            return self.Command<long>("DECR", keyName);
        }

        internal static void Discard(this IStructuredDataClient self)
        {
            self.Command<OkReply>("discard");
        }

        internal static RedisReply[] Exec(this IStructuredDataClient self)
        {
            return self.Command<RedisReply[]>("exec");
        }

        internal static bool Exists(this IStructuredDataClient self, string keyName)
        {
            return self.Command<bool>("EXISTS", keyName);
        }

        internal static void Expire(this IStructuredDataClient self, string keyName, int durationInSeconds)
        {
            self.Command<OkReply>("EXPIRE", keyName, durationInSeconds.ToString(CultureInfo.InvariantCulture));
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
                self.CommandWithPackedParameters("WATCH", keyNames, _ => _.Expect<OkReply>());
        }

        internal static T ExecuteScript<T>(this IStructuredDataClient self, string scriptSha1, IDataObject[] keys, Object[] arguments)
        {
            List<object> parameters = new List<object>();
            parameters.Add(scriptSha1);
            parameters.Add(keys.Length);
            parameters.AddRange(keys);
            parameters.AddRange(arguments);
            return self.CommandWithPackedParameters<T>("EVALSHA", parameters.ToArray());
        }
    }
}
