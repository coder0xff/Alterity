using System;
using System.Text;

namespace Doredis
{
    internal class RedisReply
    {
        public readonly object Data;
        public readonly bool IsError;

        public RedisReply(bool isError, object data)
        {
            IsError = isError;
            Data = data;
        }

        public bool IsOK
        {
            get { return IsError == false && Data.Equals("OK"); }
        }

        public bool IsQueued
        {
            get { return IsError == false && Data.Equals("QUEUED"); }
        }

        public bool IsNill
        {
            get
            {
                if (IsError) return false;
                var dataAsMultibulk = Data as RedisReply[];
                return dataAsMultibulk != null && dataAsMultibulk.Length == 0;
            }
        }

        public Object Expect(Type expectedType)
        {
            if (IsError) throw new RequestFailedException(Data.ToString());
            if (expectedType.IsInstanceOfType(Data)) return Data;
            try
            {
                if (expectedType == typeof (OkReply))
                {
                    if (!IsOK)
                        throw new RequestFailedException("Server is expected to return OK but did not. Data: " + Data);
                    if (expectedType.IsValueType)
                        return Activator.CreateInstance(expectedType);
                    else
                        return null;
                }
                else if (expectedType == typeof (QueuedReply))
                {
                    if (!IsQueued)
                        throw new RequestFailedException("Server is expected to return QUEUED but did not. Data: " +
                                                         Data);
                    if (expectedType.IsValueType)
                        return Activator.CreateInstance(expectedType);
                    else
                        return null;
                }
                else if (expectedType == typeof (RedisReply))
                {
                    return this;
                }
                else if (expectedType == typeof (bool))
                {
                    if (Data is string)
                    {
                        if ((string) Data != "0" && (string) Data != "1")
                            throw new ReplyFormatException(
                                "Expected a string or integer containing a boolean zero or one");
                        return (string) Data == "1";
                    }
                    else if (Data is long)
                    {
                        var asLong = (long) Data;
                        if (asLong != 0 && asLong != 1)
                            throw new ReplyFormatException(
                                "Expected a string or integer containing a boolean zero or one");
                        return asLong == 1;
                    }
                }
                else if (expectedType == typeof (string))
                {
                    if (Data is string)
                        return Data;
                    else if (Data is byte[])
                        return Encoding.UTF8.GetString((byte[]) Data);
                }
                else if (expectedType.IsIntegral())
                {
                    if (Data == null)
                        if (expectedType.IsValueType)
                            return Activator.CreateInstance(expectedType);
                        else
                            return null;
                    if (Data.GetType().IsIntegral())
                        return Convert.ChangeType(Data, expectedType);
                    else if (Data is byte[])
                    {
                        //Redis stores integers as strings!
                        return Convert.ChangeType(Encoding.UTF8.GetString((byte[]) Data), expectedType);
                    }
                }
                if (Data is byte[])
                {
                    return SerializationProvider.Deserialize((byte[]) Data);
                }
                else
                    return Data; //last ditch effort, just cast it
            }
            catch (InvalidCastException exc)
            {
                throw new ReplyFormatException(
                    "The returned value of type\"" + Data.GetType() + "\" could not be cast to the type \"" +
                    expectedType + "\"", exc);
            }
        }

        public T Expect<T>()
        {
            return (T) Expect(typeof (T));
        }
    }
}