using ServiceStack.Redis;
using System;
using System.Linq;

namespace Redis
{
    public sealed class Client : DynamicDataObject
    {
        class ShardCollection
        {
            ServiceStack.Redis.RedisClient[] shards;

            public ShardCollection(System.Net.IPAddress[] shardLocations)
            {
                shards = shardLocations.Select(x => new ServiceStack.Redis.RedisClient(x.ToString())).ToArray();
            }

            public ServiceStack.Redis.RedisClient SelectShard(Int32 hashCode)
            {
                int shardIndex = (Int32)(((Int64)hashCode - Int32.MinValue * shards.Length) / ((Int64)Int32.MaxValue - (Int64)Int32.MinValue + 1));
                return shards[shardIndex];
            }
        }

        ShardCollection shards;

        internal Client(System.Net.IPAddress[] shardLocations)
        {
            shards = new ShardCollection(shardLocations);
        }

        public override RedisClient GetDataStore(string memberAbsolutePath)
        {
            return shards.SelectShard(memberAbsolutePath.Split(new char[] { '.' })[0].GetHashCode());
        }

        public override string GetMemberAbsolutePath(string name, bool ignoreCase)
        {
            return name;
        }

        internal override Scope CreateScope(string memberName)
        {
            return new Scope(memberName, GetDataStore(memberName));
        }

        public override string GetAbsolutePath()
        {
            throw new InvalidOperationException();
        }
    }
}
