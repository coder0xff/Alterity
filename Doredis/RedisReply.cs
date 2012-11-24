using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doredis
{
    class RedisReply
    {
        public RedisReply(bool isError, object data)
        {
            IsError = isError;
            Data = data;
        }

        public bool IsOK { get { return IsError == false && Data.Equals("OK"); } }
        public bool IsQueued { get { return IsError == false && Data.Equals("QUEUED"); } }

        public readonly bool IsError;
        public readonly object Data;

        public T Expect<T>()
        {
            if (IsError) throw new RequestFailedException(Data.ToString());
            if (Data is T) return (T)Data;
            try
            {
                Type expectedType = typeof(T);
                if (expectedType == typeof(OkReply))
                {
                    if (!IsOK) throw new RequestFailedException("Server is expected to return OK but did not. Data: " + Data.ToString());
                    return default(T);
                }
                else if (expectedType == typeof(QueuedReply))
                {
                    if (!IsQueued) throw new RequestFailedException("Server is expected to return QUEUED but did not. Data: " + Data.ToString());
                    return default(T);
                }
                else if (expectedType == typeof(RedisReply))
                {
                    return (T)(Object)this;
                }
                else if (expectedType == typeof(bool))
                {
                    if (Data is string)
                    {
                        if ((string)Data != "0" && (string)Data != "1") throw new ReplyFormatException("Expected a string or integer containing a boolean zero or one");
                        return (T)(Object)((string)Data == "1");
                    }
                    else if (Data is long)
                    {
                        long asLong = (long)Data;
                        if (asLong != 0 && asLong != 1) throw new ReplyFormatException("Expected a string or integer containing a boolean zero or one");
                        return (T)(Object)(asLong == 1);
                    }
                }
                else if (expectedType == typeof(string))
                {
                    if (Data is string)
                        return (T)Data;
                    else if (Data is byte[])
                        return (T)(Object)Encoding.UTF8.GetString((byte[])Data);
                }
                else if (expectedType.IsIntegral())
                {
                    if (Data == null)
                        return default(T);
                    if (Data.GetType().IsIntegral())
                        return (T)(Object)System.Convert.ChangeType(Data, typeof(T));
                    else if (Data is byte[])
                    {
                        //redis stores integers as strings!
                        return (T)System.Convert.ChangeType(Encoding.UTF8.GetString((byte[])Data), typeof(T));
                    }
                }
                if (Data is byte[])
                {
                    return (T)SerializationProvider.Deserialize((byte[])Data);
                }
                else
                    return (T)Data; //last ditch effort, just cast it
            }
            catch (InvalidCastException exc)
            {
                throw new ReplyFormatException("The returned value of type\"" + Data.GetType().ToString() + "\" could not be cast to the type \"" + typeof(T).ToString() + "\"", exc);
            }
        }
    }
}
