using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;

namespace Doredis
{
    public class Lock : IDisposable
    {
        private const int KeepAliveExpireTime = 10;
        //if the server that held the lock goes down, this'll keep all the other servers from waiting forever

        private static readonly string AcquireLock = @"
	        redis.call('del', '.lockProcPreceding_' .. LOCK_TO_SIGNAL)
	        redis.call('sunionstore', '.lockKeys', '.lockKeys', '.lockProcKeys_' .. LOCK_TO_SIGNAL)
	        redis.call('setex', '.lockKeepAlive_' .. LOCK_TO_SIGNAL, '" + KeepAliveExpireTime.ToString(CultureInfo.InvariantCulture) + @"', ''))
	        redis.call('publish', '.lockProcSignal_' .. LOCK_TO_SIGNAL, ABANDONED_KEYS)
        ";

        private static readonly string CheckForAquireWithAbandoned = @"
	        local guid = ARGV[1]
	        local failedProcCount = 0
	        for index, precedingProcGuid in pairs(redis.call('smembers', 'lockProcSubsequent_' .. guid)) do
		        if redis.call('EXISTS', '.lockKeepAlive_' .. guid) == 0 then
			        failedProcCount = failedProcCount + 1
		        end
	        end
	        if failedProcCount == redis.call('scard', '.lockProcPreceding_' + guid) then
		        local LOCK_TO_SIGNAL = guid
		        local ABANDONED_KEYS = ''
		        for index, abandonedKey in pairs(redic.call('sinter', '.lockKeys', '.lockProcKeys_' .. guid)) do
			        ABANDONED_KEYS = ABANDONED_KEYS .. abandonedKey .. ' '
		        end" +
		        AcquireLock +
	        @"end
        ";

        private const string FreeLockScript = @"
	        local guid = ARGV[1]
	        redis.call('del', '.lockKeepAlive_' .. guid)
	        redis.call('sdiffstore', '.lockKeys', '.lockKeys', '.lockProcKeys_' .. guid)
	        for index, subsequentProcGuid in pairs(redis.call('smembers', '.lockProcSubsequent_' .. guid)) do
		        redis.call('srem', '.lockProcPreceding_' .. subsequentProcGuid, guid)
		        if redis.call('scard', '.lockProcPreceding_' .. subsequentProcGuid) == 0 then
			        local LOCK_TO_SIGNAL = subsequentProcGuid
			        local ABANDONED_KEYS = ''
			        SIGNAL_LOCK()
		        end
	        end
	        redis.call('del', '.lockProcKeys_' .. guid, '.lockProcSubsequent_' .. guid)
        ";

        private static readonly String RequestLockScript = @"
	        local guid = ARGV[1]
	        redis.call('DEL', '.lockProcSubsequent_' .. guid, '.lockProcPreceding_' .. guid, '.lockProcKeys_' .. guid)
	        for index, keyName in ipairs(KEYS) do
		        if redis.call('SISMEMBER', '.lockKeys', keyName) == 0 then
			        redis.call('DEL', 'lockKeyLeaf_' .. keyName)
		        end
		        if redis.call('EXISTS', '.lockKeyLeaf_' .. guid) == 1 then
			        local precedingProcGuid = redis.call('get', '.lockKeyLeaf_' .. keyName)
			        redis.call('SADD', '.lockProcPreceding_' .. guid, precedingProcGuid)
			        redis.call('SADD', '.lockProcSubsequent_' .. precedingProcGuid, guid)
		        end
		        redis.call('SADD', '.lockProcKeys_' .. guid, keyName)
		        redis.call('SET', '.lockKeyLeaf_' .. keyName, guid)
	        end
	        if redis.call('SCARD', '.lockProcPreceding_' .. guid) == 0
		        local LOCK_TO_SIGNAL = guid
		        local ABANDONED_KEYS = ''" +
                AcquireLock +
            @"end
        ";

        private static readonly String AbortLockScript = @"
	        local guid = ARGV[1]
	        if redis.call('SCARD', '.lockProcPreceding_' .. guid) == 0 then
		        return 0
	        else
		        for index0, subsequentProcGuid in pairs(redis.call('SMEMBERS', '.lockProcSubsequent_' .. guid)) do
			        for index1, precedingProcGuid in pairs(redis.call('SMEMBERS', '.lockProcPreceding_' .. guid)) do
				        redis.call('SADD', '.lockProcSubsequent_' .. precedingProcGuid, subsequentProcGuid)
				        redis.call('SADD', '.lockProcPreceding_' .. subsequentProcGuid, precedingProcGuid)
			        end
		        end
		        for index, precedingProcGuid in pairs(redis.call('SMEMBERS', '.lockProcPreceding_' .. guid)) do
			        redis.call('SREM', '.lockProcSubsequent_' .. precedingProcGuid, guid)
		        end
		        for index, subsequentProcGuid in pairs(redis.call('SMEMBERS', '.lockProcSubsequent_' .. guid)) do
			        redis.call('SREM', '.lockProcPreceding_' .. subsequentProcGuid, guid)
			        if redis.call('SCARD', '.lockProcPreceding_' .. subsequentProcGuid) == 0 then
				        local LOCK_TO_ACTIVATE = subsequentProcGuid
				        ACTIVATE_LOCK
				        redis.call('publish', '.lockProcSignal_' .. subsequentProcGuid, '')
			        end
		        end
		        redis.call('DEL', '.lockProcPreceding_' + guid, '.lockProcSubsequent_' + guid)
		        return 1
	        end
        ";

        private static readonly String CheckForAcquireWithAbandonedScriptSha1 = CheckForAquireWithAbandoned.Utf8Sha1Hash();

        private static readonly String FreeLockScriptSha1 = FreeLockScript.Utf8Sha1Hash();

        private static readonly String RequestLockScriptSha1 = RequestLockScript.Utf8Sha1Hash();

        private static readonly String AbortLockScriptSha1 = AbortLockScript.Utf8Sha1Hash();

        private readonly String _guid;
        private readonly DataStoreShard _lockConductor;
        private readonly ManualResetEventSlim _signal;
        private String[] _abandonedKeys;
        private DateTime _lastKeepAliveTime;
        private int _waiting;
        private bool _success;
        private readonly Action<string> _signaledHandler;

        public Lock(IList<object> dataObjects, int millisecondsTimeout)
        {
            var objects = new IDataObject[dataObjects.Count];
            for (int index = 0; index < dataObjects.Count; index++)
            {
                var o = dataObjects[index] as IDataObject;
                if (o != null)
                    objects[index] = o;
                else
                    throw new NonDataObjectException();
            }
            DataStore dataStore = objects[0].GetDataStore();
            _lockConductor = dataStore.LockConductorShard();
            for (int index = 1; index < dataObjects.Count; index++)
                if (objects[index].GetDataStore() != dataStore)
                    throw new DifferentDataStoresException();

            _guid = Guid.NewGuid().ToString();            
            _waiting = 1;
            _success = false;
            _signal = new ManualResetEventSlim(false);
            _abandonedKeys = new string[0];
            _signaledHandler = s =>
                {
                    if (Interlocked.CompareExchange(ref _waiting, 0, 1) == 1)
                    {
                        _abandonedKeys = s.Split(new[] {' '});
                        _success = true;
                        _signal.Set();
                    }
                };
            _lockConductor.Subscribe(".lockProcSignal_" + _guid, _signaledHandler);
            _lockConductor.UploadScript(RequestLockScript, RequestLockScriptSha1);
            var lockRequestReply = _lockConductor.ExecuteScript<RedisReply>(scriptSha1, objects, new object[] {_guid});
            if (!lockRequestReply.IsNill && lockRequestReply.Data is String)
            {
                //lock acquired immediately
                _abandonedKeys = (lockRequestReply.Data as String).Split(new[] {' '});
                _signal.Set();
            }
            _signal.WaitOne(millisecondsTimeout);
            lock (_guid)
            {
                if (_success)
                {
                    _lockConductor.Unsubscribe(".lockProcSignal_" + _guid, _signaledHandler);
                }
                else
                {
                    _lockConductor.UploadScript(AbortLockScript, AbortLockScriptSha1);
                    _lockConductor.ExecuteScript<RedisReply>()
                }
            }
            _lastKeepAliveTime = DateTime.Now;
            if (_abandonedKeys.Length != 0)
                throw new AbandonedLockException(_abandonedKeys);
            if (!_success)
                throw new TimeoutException("The requested keys were not locked within the specified timeout duration.");
        }

        public void Dispose()
        {
            if (_success)
            _lockConductor.UploadScript(FreeLockScript, scriptSha1);
            _lockConductor.ExecuteScript<RedisReply>(FreeLockScriptSha1, new IDataObject[0], new object[] {guid});
            _signal.Dispose();
        }

        public void KeepAlive()
        {
            if (!((DateTime.Now - _lastKeepAliveTime).TotalSeconds > KeepAliveExpireTime/2)) return;
            _lockConductor.UploadScript(CheckForAquireWithAbandoned, CheckForAcquireWithAbandonedScriptSha1);
            _lockConductor.ExecuteScript<RedisReply>(CheckForAcquireWithAbandonedScriptSha1, new IDataObject[0], new object[] {guid});
            _lastKeepAliveTime = DateTime.Now;
        }
    }

    [Serializable]
    public class AbandonedLockException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public AbandonedLockException(String[] abandonedKeys)
        {
            AbandonedKeys = abandonedKeys;
        }

        protected AbandonedLockException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public string[] AbandonedKeys { get; private set; }
    }

    [Serializable]
    public class NonDataObjectException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public NonDataObjectException()
        {
        }

        protected NonDataObjectException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public class DifferentDataStoresException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public DifferentDataStoresException() : this("All data objects must be from the same DataStore")
        {
        }

        private DifferentDataStoresException(string message) : base(message)
        {
        }

        protected DifferentDataStoresException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}