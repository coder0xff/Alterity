using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Doredis
{
    [Serializable]
    public class TransactionFailureException : Exception
    {
        public TransactionFailureException() { }
        public TransactionFailureException(string message) : base(message) { }
        public TransactionFailureException(string message, Exception inner) : base(message, inner) { }
        protected TransactionFailureException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class CompletedOrCancelledTransactionException : Exception
    {
        public CompletedOrCancelledTransactionException() { }
        public CompletedOrCancelledTransactionException(string message) : base(message) { }
        public CompletedOrCancelledTransactionException(string message, Exception inner) : base(message, inner) { }
        protected CompletedOrCancelledTransactionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    class Transaction
    {
        [Serializable]
        public class TransactionCancelledException : Exception
        {
            public TransactionCancelledException() { }
            public TransactionCancelledException(string message) : base(message) { }
            public TransactionCancelledException(string message, Exception inner) : base(message, inner) { }
            protected TransactionCancelledException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
        }

        internal interface ITransactionClient : IStructuredDataAsyncClient
        {
            Exception Cancel();
        }

        class TransactionAsyncClient : ITransactionClient, IDisposable
        {
            List<byte[]> compiledCommands;
            List<Action<RedisReply>> resultDispatchers;
            IStructuredDataClient client;

            public TransactionAsyncClient(IStructuredDataClient client, List<byte[]> compiledCommands, List<Action<RedisReply>> resultDispatchers)
            {
                this.compiledCommands = compiledCommands;
                this.resultDispatchers = resultDispatchers;
                this.client = client;
            }

            public void SendCommandWithPackedObjects(string command, object[] arguments)
            {
                compiledCommands.Add(RedisProtocolClient.EncodeCommandWithPackedObjects(command, arguments));
                resultDispatchers.Add(null);
            }

            public void CommandWithPackedParameters(string command, object[] arguments, Action<RedisReply> resultHandler = null)
            {
                if (compiledCommands == null)
                    throw new CompletedOrCancelledTransactionException("TransactionBuilder");
                compiledCommands.Add(RedisProtocolClient.EncodeCommandWithPackedObjects(command, arguments));
                resultDispatchers.Add(resultHandler);
            }

            public Exception Cancel()
            {
                return new TransactionCancelledException();
            }

            public void Dispose()
            {
                compiledCommands = null;
            }

            public System.Net.HostEndPoint EndPoint
            {
                get { return client.EndPoint; }
            }
        }

        List<byte[]> compiledCommands = new List<byte[]>();
        List<Action<RedisReply>> resultDispatchers = new List<Action<RedisReply>>();
        IStructuredDataClient client;
        HashSet<string> watchedKeys = new HashSet<string>();
        byte[] compiledTransaction;

        internal Transaction(IStructuredDataClient client, Action<ITransactionClient> transactionBuilder)
        {
            this.client = client;
            using (TransactionAsyncClient builder = new TransactionAsyncClient(client, compiledCommands, resultDispatchers))
            {
                transactionBuilder(builder);
            }
            Compile();
        }

        void Compile()
        {
            MemoryStream compileStream = new MemoryStream();
            foreach (byte[] compiledCommand in compiledCommands)
                compileStream.Write(compiledCommand, 0, compiledCommand.Length);
            compiledTransaction = compileStream.ToArray();
        }

        internal void Execute()
        {
            client.Watch(watchedKeys.ToArray());
            client.Multi();
            try
            {
                client.SendRaw(compiledTransaction);
                for (int commandIndex = 0; commandIndex < compiledCommands.Count; commandIndex++)
                {
                    RedisReply queuedCommandReply = client.ReadReply();
                    try
                    {
                        queuedCommandReply.Expect<QueuedReply>();
                    }
                    catch (RequestFailedException)
                    {
                        throw new TransactionFailureException("Command index " + commandIndex.ToString() + " failed to queue. Data: " + queuedCommandReply.Data.ToString());
                    }
                }
            }
            catch(Exception)
            {
                client.Discard();
                throw;
            }
            RedisReply[] transactionResults = client.Exec();
            if (resultDispatchers.Count != transactionResults.Length) throw new TransactionFailureException(resultDispatchers.Count.ToString() + "command(s) were queued to the transaction, and " + transactionResults.Length.ToString() + " replies were received.");
            for (int resultIndex = 0; resultIndex < transactionResults.Length; resultIndex++)
            {
                try
                {
                    if (resultDispatchers[resultIndex] != null)
                        resultDispatchers[resultIndex](transactionResults[resultIndex]);
                }
                catch (ReplyFormatException)
                {
                }
                catch (RequestFailedException)
                {
                }
            }
        }
    }
}
