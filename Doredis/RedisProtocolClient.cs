using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;

namespace Doredis
{
    [Serializable]
    public class FailedToConnectException : Exception
    {
        public FailedToConnectException() { }
        protected FailedToConnectException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class ReplyFormatException : Exception
    {
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
        protected RequestTimeoutException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class RequestFailedException : Exception
    {
        public RequestFailedException(string message) : base(message) { }
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

        readonly System.Net.HostEndPoint _endPoint;
        public System.Net.HostEndPoint EndPoint
        {
            get { return _endPoint; }
        }

        readonly TcpClient _tcpClient;
        private readonly bool _connected;
        readonly NetworkStream _tcpClientStream;
        readonly ConcurrentQueue<byte[]> _replyStreamBlockQueue = new ConcurrentQueue<byte[]>();
        internal readonly ManualResetEvent ReplyStreamBlockReady = new ManualResetEvent(false);
        byte[] _replyStreamBlock;
        int _replyStreamBlockPosition;

        RedisProtocolClient(System.Net.HostEndPoint endPoint, int millisecondsTimeout)
        {
            _endPoint = endPoint;
            _tcpClient = new TcpClient();
            if (_tcpClient.Connect(endPoint.Host, endPoint.Port, millisecondsTimeout))
            {
                _connected = true;
                _tcpClientStream = _tcpClient.GetStream();
                BeginRead();
            }
        }

        internal static RedisProtocolClient Create(System.Net.HostEndPoint endPoint, int millisecondsTimeout = 1000)
        {
            var result = new RedisProtocolClient(endPoint, millisecondsTimeout);
            if (!result._connected)
                throw new FailedToConnectException();
            return result;
        }

        void BeginRead()
        {
            var buffer = new byte[_tcpClient.ReceiveBufferSize];
            Debug.Assert(_tcpClientStream != null, "_tcpClientStream != null");
            _tcpClientStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        static void EncodeRawArgumentsToStream(System.IO.Stream stream, byte[][] arguments)
        {
            if (arguments.Length == 0) return;
            stream.WriteUtf8("*", arguments.Length.ToString(CultureInfo.InvariantCulture), LineEnd);
            foreach (byte[] t in arguments)
            {
                stream.WriteUtf8("$", t.Length.ToString(CultureInfo.InvariantCulture), LineEnd);
                stream.Write(t, 0, t.Length);
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
            var serializedArguments = new byte[arguments.Length][];
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

/*
        static void EncodePackedObjectsToStream(System.IO.Stream stream, object[] arguments)
        {
            EncodeRawArgumentsToStream(stream, SerializeArguments(arguments));
        }
*/

        static byte[] EncodePackedObjects(object[] arguments)
        {
            return EncodeRawArguments(SerializeArguments(arguments));
        }

        static IEnumerable<byte> EncodeObjects(params object[] arguments)
        {
            return EncodePackedObjects(arguments);
        }

/*
        internal static void EncodeCommandWithPackedObjectsToStream(System.IO.Stream stream, string command, object[] arguments)
        {
            var commandAndArguments = new object[arguments.Length + 1];
            commandAndArguments[0] = command;
            arguments.CopyTo(commandAndArguments, 1);
            EncodePackedObjectsToStream(stream, commandAndArguments);
        }
*/

        internal static byte[] EncodeCommandWithPackedObjects(string command, IEnumerable<object> arguments)
        {
            Debug.Assert(arguments != null, "arguments != null");
            var enumerable = arguments as object[] ?? arguments.ToArray();
            var commandAndArguments = new object[enumerable.Count() + 1];
            commandAndArguments[0] = command;
            enumerable.CopyTo(commandAndArguments, 1);
            return EncodePackedObjects(commandAndArguments);
        }

/*
        internal static void EncodeCommandWithObjectsToStream(System.IO.Stream stream, string command, params object[] arguments)
        {
            EncodeCommandWithPackedObjectsToStream(stream, command, arguments);
        }
*/

/*
        internal static byte[] EncodeCommandWithObjects(string command, params object[] arguments)
        {
            return EncodeCommandWithPackedObjects(command, arguments);
        }
*/

        internal void SendPackedObjects(object[] arguments)
        {
            byte[] buffer = EncodeObjects(arguments).ToArray();
            _tcpClientStream.WriteAsync(buffer, 0, buffer.Length);
        }

/*
        void SendRaw(byte[][] arguments)
        {
            SendRaw(EncodeRawArguments(arguments).ToArray());
        }
*/

        internal void SendRaw(byte[] data)
        {
            _tcpClientStream.WriteAsync(data, 0, data.Length);
        }

        internal void SendCommandWithPackedObjects(string command, IEnumerable<object> arguments)
        {
            SendRaw(EncodeCommandWithPackedObjects(command, arguments));
        }

        void ReadCallback(IAsyncResult result)
        {
            int readLength;
            try
            {
                readLength = _tcpClientStream.EndRead(result);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            if (readLength != 0)
            {
                var buffer = result.AsyncState as byte[];
                var block = new byte[readLength];
                Buffer.BlockCopy(buffer, 0, block, 0, readLength);
                _replyStreamBlockQueue.Enqueue(block);
                ReplyStreamBlockReady.Set();
            }
            BeginRead();
        }

        internal bool DataIsReady()
        {
            return !(_replyStreamBlock == null || _replyStreamBlockPosition >= _replyStreamBlock.Length) || _replyStreamBlockQueue.Count > 0;
        }

        private void WaitForData(bool noTimeout = false)
        {
            while (_replyStreamBlock == null || _replyStreamBlockPosition >= _replyStreamBlock.Length)
            {
                ReplyStreamBlockReady.Reset();
                if (_replyStreamBlockQueue.TryDequeue(out _replyStreamBlock))
                    _replyStreamBlockPosition = 0;

                else
                {
                    if (noTimeout)
                        ReplyStreamBlockReady.WaitOne();
                    else if (!ReplyStreamBlockReady.WaitOne(1000))
                    {
                        if (_replyStreamBlockQueue.Count == 0)
                            throw new RequestTimeoutException();
                    }
                }
            }
        }

        byte ReadReplyByte()
        {
            WaitForData();
            return _replyStreamBlock[_replyStreamBlockPosition++];
        }

        byte[] ReadReplyBytes(int count)
        {
            var result = new byte[count];
            int destinationIndex = 0;
            while (destinationIndex < count)
            {
                WaitForData();
                int desiredCount = count - destinationIndex;
                int availableCount = _replyStreamBlock.Length - _replyStreamBlockPosition;
                int copyCount = Math.Min(desiredCount, availableCount);
                Buffer.BlockCopy(_replyStreamBlock, _replyStreamBlockPosition, result, destinationIndex, copyCount);
                destinationIndex += copyCount;
                _replyStreamBlockPosition += copyCount;
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
            var utf8String = new List<byte>();
            while (true)
            {
                byte b = ReadReplyByte();
                if (b == Utf8CarriageReturn)
                {
                    b = ReadReplyByte();
                    if (b == Utf8LineFeed)
                        return Encoding.UTF8.GetString(utf8String.ToArray());
                    utf8String.Add(Utf8CarriageReturn);
                    utf8String.Add(b);
                }
                else
                {
                    utf8String.Add(b);
                }
            }
        }

        long ReadReplyInteger()
        {
            return long.Parse(ReadReplyTextLine());
        }

        byte[] ReadReplyBulk()
        {
            var byteCount = (int)ReadReplyInteger();
            if (byteCount == -1) return null;
            byte[] result = ReadReplyBytes(byteCount);
            ReadReplyTextLine();
            return result;
        }

        RedisReply[] ReadReplyMultiBulk()
        {
            var bulkCount = (int)ReadReplyInteger();
            var result = new RedisReply[bulkCount];
            for (int i = 0; i < bulkCount; i++)
            {
                result[i] = ReadReply();
            }
            return result;
        }

        public void Dispose()
        {
            _tcpClient.Close();
            ReplyStreamBlockReady.Dispose();
        }

        void IStructuredDataClient.SendRaw(byte[] data)
        {
            throw new NotImplementedException();
        }

        RedisReply IStructuredDataClient.ReadReply()
        {
            throw new NotImplementedException();
        }

        public void CommandWithPackedParameters(string command, IEnumerable<object> arguments, Action<RedisReply> resultHandler)
        {
            SendCommandWithPackedObjects(command, arguments);
            resultHandler(ReadReply());
        }
    }
}
