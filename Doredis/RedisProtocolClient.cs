using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;

namespace Doredis
{
    [Serializable]
    public class FailedToConnectException : Exception
    {
        public FailedToConnectException() { }
        public FailedToConnectException(string message) : base(message) { }
        public FailedToConnectException(string message, Exception inner) : base(message, inner) { }
        protected FailedToConnectException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class RequestFormatException : Exception
    {
        public RequestFormatException() { }
        public RequestFormatException(string message) : base(message) { }
        public RequestFormatException(string message, Exception inner) : base(message, inner) { }
        protected RequestFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class ReplyFormatException : Exception
    {
        public ReplyFormatException() { }
        public ReplyFormatException(string message) : base(message) { }
        public ReplyFormatException(string message, Exception inner) : base(message, inner) { }
        protected ReplyFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class RequestTimeoutException : Exception
    {
        public RequestTimeoutException() { }
        public RequestTimeoutException(string message) : base(message) { }
        public RequestTimeoutException(string message, Exception inner) : base(message, inner) { }
        protected RequestTimeoutException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class RequestFailedException : Exception
    {
        public RequestFailedException() { }
        public RequestFailedException(string message) : base(message) { }
        public RequestFailedException(string message, Exception inner) : base(message, inner) { }
        protected RequestFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    class RedisProtocolClient : IStructuredDataClient, IDisposable
    {
        const string LineEnd = "\r\n";
        const byte Utf8Plus = 0x2B;
        const byte Utf8Minus = 0x2D;
        const byte Utf8Colon = 0x3A;
        const byte Utf8DollarSign = 0x24;
        const byte Utf8Asterisk = 0x2A;
        const byte Utf8CarriageReturn = 0x0D;
        const byte Utf8LineFeed = 0x0A;

        TcpClient tcpClient;
        internal readonly bool Connected;
        NetworkStream tcpClientStream;
        IAsyncResult readAsyncResult;
        ConcurrentQueue<byte[]> replyStreamBlockQueue = new ConcurrentQueue<byte[]>();
        ManualResetEvent replyStreamBlockReady = new ManualResetEvent(false);
        byte[] replyStreamBlock;
        int replyStreamBlockPosition;

        RedisProtocolClient(string host, int port, int millisecondsTimeout)
        {
            tcpClient = new TcpClient();
            if (tcpClient.Connect(host, port, millisecondsTimeout))
            {
                Connected = true;
                tcpClientStream = tcpClient.GetStream();
                BeginRead();
            }
        }

        internal static RedisProtocolClient Create(string host, int port = 6379, int millisecondsTimeout = 1000)
        {
            RedisProtocolClient result = new RedisProtocolClient(host, port, millisecondsTimeout);
            if (!result.Connected)
                throw new FailedToConnectException();
            return result;
        }

        void BeginRead()
        {
            byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
            readAsyncResult = tcpClientStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        static void EncodeRawArgumentsToStream(System.IO.Stream stream, byte[][] arguments)
        {
            if (arguments.Length == 0) return;
            stream.WriteUtf8("*", arguments.Length.ToString(), LineEnd);
            for (int argumentIndex = 0; argumentIndex < arguments.Length; argumentIndex++)
            {
                stream.WriteUtf8("$", arguments[argumentIndex].Length.ToString(), LineEnd);
                stream.Write(arguments[argumentIndex], 0, arguments[argumentIndex].Length);
                stream.WriteUtf8(LineEnd);
            }
        }

        static byte[] EncodeRawArguments(byte[][] arguments)
        {
            var stream = new System.IO.MemoryStream();
            EncodeRawArgumentsToStream(stream, arguments);
            return stream.ToArray();
        }

        static byte[][] SerializeArguments(object[] arguments)
        {
            byte[][] serializedArguments = new byte[arguments.Length][];
            for (int index = 0; index < arguments.Length; index++)
            {
                object argument = arguments[index];
                Type argumentType = argument.GetType();
                if (argumentType == typeof(byte[]))
                    serializedArguments[index] = (byte[])argument;
                else if (argumentType == typeof(string))
                    serializedArguments[index] = Encoding.UTF8.GetBytes((string)argument);
                else if (argumentType.IsIntegral())
                    serializedArguments[index] = Encoding.UTF8.GetBytes(argument.ToString());
                else
                    serializedArguments[index] = SerializationProvider.Serialize(argument);
            }
            return serializedArguments;
        }

        static void EncodePackedObjectsToStream(System.IO.Stream stream, object[] arguments)
        {
            EncodeRawArgumentsToStream(stream, SerializeArguments(arguments));
        }

        static byte[] EncodePackedObjects(object[] arguments)
        {
            return EncodeRawArguments(SerializeArguments(arguments));
        }

        static byte[] EncodeObjects(params object[] arguments)
        {
            return EncodeObjects(arguments);
        }

        internal static void EncodeCommandWithPackedObjectsToStream(System.IO.Stream stream, string command, object[] arguments)
        {
            object[] commandAndArguments = new object[arguments.Length + 1];
            commandAndArguments[0] = command;
            arguments.CopyTo(commandAndArguments, 1);
            EncodePackedObjectsToStream(stream, commandAndArguments);
        }

        internal static byte[] EncodeCommandWithPackedObjects(string command, object[] arguments)
        {
            object[] commandAndArguments = new object[arguments.Length + 1];
            commandAndArguments[0] = command;
            arguments.CopyTo(commandAndArguments, 1);
            return EncodePackedObjects(commandAndArguments);
        }

        internal static void EncodeCommandWithObjectsToStream(System.IO.Stream stream, string command, params object[] arguments)
        {
            EncodeCommandWithPackedObjectsToStream(stream, command, arguments);
        }

        internal static byte[] EncodeCommandWithObjects(string command, params object[] arguments)
        {
            return EncodeCommandWithPackedObjects(command, arguments);
        }

        internal void SendPackedObjects(object[] arguments)
        {
            byte[] buffer = EncodeObjects(arguments).ToArray();
            tcpClientStream.WriteAsync(buffer, 0, buffer.Length);
        }

        void SendRaw(byte[][] arguments)
        {
            SendRaw(EncodeRawArguments(arguments).ToArray());
        }

        internal void SendRaw(byte[] data)
        {
            tcpClientStream.WriteAsync(data, 0, data.Length);
        }

        internal void SendCommandWithPackedObjects(string command, object[] arguments)
        {
            SendRaw(EncodeCommandWithPackedObjects(command, arguments));
        }


        void ReadCallback(IAsyncResult result)
        {
            int readLength = 0;
            try
            {
                readLength = tcpClientStream.EndRead(result);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            if (readLength != 0)
            {
                byte[] buffer = result.AsyncState as byte[];
                byte[] block = new byte[readLength];
                Buffer.BlockCopy(buffer, 0, block, 0, readLength);
                replyStreamBlockQueue.Enqueue(block);
                replyStreamBlockReady.Set();
            }
            BeginRead();
        }

        internal bool WaitForData(bool noTimeout = false)
        {
            while (replyStreamBlock == null || replyStreamBlockPosition >= replyStreamBlock.Length)
            {
                replyStreamBlockReady.Reset();
                if (replyStreamBlockQueue.TryDequeue(out replyStreamBlock))
                    replyStreamBlockPosition = 0;

                else
                {
                    if (noTimeout)
                        replyStreamBlockReady.WaitOne();
                    else if (!replyStreamBlockReady.WaitOne(1000))
                        throw new RequestTimeoutException();
                }
            }
            return true;
        }

        byte ReadReplyByte()
        {
            WaitForData();
            return replyStreamBlock[replyStreamBlockPosition++];
        }

        byte[] ReadReplyBytes(int count)
        {
            byte[] result = new byte[count];
            int destinationIndex = 0;
            while (destinationIndex < count)
            {
                WaitForData();
                int desiredCount = count - destinationIndex;
                int availableCount = replyStreamBlock.Length - replyStreamBlockPosition;
                int copyCount = Math.Min(desiredCount, availableCount);
                Buffer.BlockCopy(replyStreamBlock, replyStreamBlockPosition, result, destinationIndex, copyCount);
                destinationIndex += copyCount;
                replyStreamBlockPosition += copyCount;
            }
            return result;
        }

        internal RedisReply ReadReply()
        {
            while (true)
            {
                switch (ReadReplyByte())
                {
                    case Utf8Plus:
                        return new RedisReply(false, ReadReplyTextLine());
                    case Utf8Minus:
                        return new RedisReply(true, ReadReplyTextLine());
                    case Utf8Colon:
                        return new RedisReply(false, ReadReplyInteger());
                    case Utf8DollarSign:
                        return new RedisReply(false, ReadReplyBulk());
                    case Utf8Asterisk:
                        return new RedisReply(false, ReadReplyMultiBulk());
                    default:
                        throw new ReplyFormatException("Unrecognized reply type");
                }
            }
        }

        string ReadReplyTextLine()
        {
            List<byte> Utf8String = new List<byte>();
            while (true)
            {
                byte b = ReadReplyByte();
                if (b == Utf8CarriageReturn)
                {
                    b = ReadReplyByte();
                    if (b == Utf8LineFeed)
                    {
                        return Encoding.UTF8.GetString(Utf8String.ToArray());
                    }
                    else
                    {
                        Utf8String.Add(Utf8CarriageReturn);
                        Utf8String.Add(b);
                    }
                }
                else
                {
                    Utf8String.Add(b);
                }
            }
        }

        long ReadReplyInteger()
        {
            return long.Parse(ReadReplyTextLine());
        }

        byte[] ReadReplyBulk()
        {
            int byteCount = (int)ReadReplyInteger();
            byte[] result = ReadReplyBytes(byteCount);
            ReadReplyTextLine();
            return result;
        }

        RedisReply[] ReadReplyMultiBulk()
        {
            int bulkCount = (int)ReadReplyInteger();
            RedisReply[] result = new RedisReply[bulkCount];
            for (int i = 0; i < bulkCount; i++)
            {
                result[i] = ReadReply();
            }
            return result;
        }

        public void Dispose()
        {
            tcpClient.Close();
            replyStreamBlockReady.Dispose();
        }

        void IStructuredDataClient.SendRaw(byte[] data)
        {
            throw new NotImplementedException();
        }

        RedisReply IStructuredDataClient.ReadReply()
        {
            throw new NotImplementedException();
        }

        public void Command(string command, object[] arguments, Action<RedisReply> resultHandler)
        {
            SendCommandWithPackedObjects(command, arguments);
            resultHandler(ReadReply());
        }
    }
}
