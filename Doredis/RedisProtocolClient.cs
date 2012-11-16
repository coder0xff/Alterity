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

    public class RedisReply
    {
        public RedisReply(bool isError, object data)
        {
            IsError = isError;
            Data = data;
        }

        public readonly bool IsError;
        public readonly object Data;
    }

    public class RedisProtocolClient : IDisposable
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
        public readonly bool Connected;
        NetworkStream tcpClientStream;
        IAsyncResult readAsyncResult;
        ConcurrentQueue<byte[]> replyStreamBlockQueue = new ConcurrentQueue<byte[]>();
        ManualResetEvent replyStreamBlockReady = new ManualResetEvent(false);
        byte[] replyStreamBlock;
        int replyStreamBlockPosition;

        private RedisProtocolClient(string host, int port, int millisecondsTimeout)
        {
            tcpClient = new TcpClient();
            if (tcpClient.Connect(host, port, millisecondsTimeout))
            {
                Connected = true;
                tcpClientStream = tcpClient.GetStream();
                BeginRead();
            }
        }

        public static RedisProtocolClient Create(string host, int port = 6379, int millisecondsTimeout = 1000)
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

        internal void Send(params object[] arguments)
        {
            byte[][] encodedArguments = new byte[arguments.Length][];
            for (int index = 0; index < arguments.Length; index++)
            {
                if (arguments.GetType() == typeof(byte[]))
                    encodedArguments[index] = (byte[])arguments[index];
                else if (arguments.GetType() == typeof(string))
                    encodedArguments[index] = Encoding.UTF8.GetBytes((string)arguments[index]);
                else
                    encodedArguments[index] = SerializationProvider.Serialize(arguments[index]);
            }
            SendArguments(encodedArguments);
        }

        public void SendArguments(byte[][] arguments)
        {
            var tempStream = new System.IO.MemoryStream();
            if (arguments.Length == 0) return;
            tempStream.WriteUtf8("*", arguments.Length.ToString(), LineEnd);
            for (int argumentIndex = 0; argumentIndex < arguments.Length; argumentIndex++)
            {
                tempStream.WriteUtf8("$", arguments[argumentIndex].Length.ToString(), LineEnd);
                tempStream.Write(arguments[argumentIndex], 0, arguments[argumentIndex].Length);
                tempStream.WriteUtf8(LineEnd);
            }
            tempStream.Seek(0, System.IO.SeekOrigin.Begin);
            byte[] buffer = tempStream.ToArray();
            tcpClientStream.WriteAsync(buffer, 0, buffer.Length);
        }

        public void SendCommand(string command, byte[][] arguments)
        {
            if (command.Contains(LineEnd)) throw new RequestFormatException("commands may not contain carriage returns");
            var tempStream = new System.IO.MemoryStream();
            if (arguments.Length == 0) return;
            tempStream.WriteUtf8("*", (arguments.Length + 1).ToString(), LineEnd);
            tempStream.WriteUtf8("$", command.Length.ToString(), LineEnd, command, LineEnd);
            for (int argumentIndex = 0; argumentIndex < arguments.Length; argumentIndex++)
            {
                tempStream.WriteUtf8("$", arguments[argumentIndex].Length.ToString(), LineEnd);
                tempStream.Write(arguments[argumentIndex], 0, arguments[argumentIndex].Length);
                tempStream.WriteUtf8(LineEnd);
            }
            tempStream.Seek(0, System.IO.SeekOrigin.Begin);
            byte[] buffer = tempStream.ToArray();
            tcpClientStream.WriteAsync(buffer, 0, buffer.Length);
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

        public bool WaitForData(bool noTimeout = false)
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

        public RedisReply ReadReply()
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
    }
}
