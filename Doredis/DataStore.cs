using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doredis
{
    public class DataStore : DynamicDataObject
    {
        class ShardCollection
        {
            DataStoreShard[] shards;

            public ShardCollection(System.Net.HostEndPoint[] shardLocations)
            {
                shards = shardLocations.Select(x => new DataStoreShard(x)).ToArray();
            }

            public DataStoreShard SelectShard(Int32 hashCode)
            {
                int shardIndex = (Int32)(((Int64)hashCode - Int32.MinValue * shards.Length) / ((Int64)Int32.MaxValue - (Int64)Int32.MinValue + 1));
                return shards[shardIndex];
            }
        }

        ShardCollection shards;

        public DataStore(System.Net.HostEndPoint[] shardLocations)
        {
            shards = new ShardCollection(shardLocations);
        }

        internal override DataStoreShard GetDataStoreShard(string memberAbsolutePath)
        {
            return shards.SelectShard(memberAbsolutePath.Split(new char[] { '.' })[0].GetHashCode());
        }

        internal override string GetMemberAbsolutePath(string name, bool ignoreCase)
        {
            return name;
        }

        internal override Scope CreateScope(string memberName)
        {
            return new Scope(memberName, GetDataStoreShard(memberName));
        }

        internal override string GetAbsolutePath()
        {
            throw new InvalidOperationException();
        }
    }
}
