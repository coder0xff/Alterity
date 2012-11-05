using ServiceStack.Redis;
using System;
using System.Linq;

namespace Redis
{
    public sealed class Service : DynamicDataObject
    {
        class ShardCollection
        {
            ServiceStack.Redis.RedisClient[] shards;

            public ShardCollection(Uri[] shardLocations)
            {
                shards = shardLocations.Select(x => new ServiceStack.Redis.RedisClient(x)).ToArray();
            }

            public ServiceStack.Redis.RedisClient SelectShard(Int32 hashCode)
            {
                int shardIndex = (Int32)((Int64)hashCode + Int32.MinValue * shards.Length / ((Int64)Int32.MaxValue - (Int64)Int32.MinValue + 1));
                return shards[shardIndex];
            }
        }

        ShardCollection shards;

        public Service(string connectionStringName)
            : this(GetShardUris(connectionStringName))
        {
        }

        public Service(Uri[] shardLocations)
        {
            shards = new ShardCollection(shardLocations);
        }

        static Uri[] GetShardUris(string connectionStringName)
        {
            System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            System.Configuration.ConnectionStringSettings connString =
                rootWebConfig.ConnectionStrings.ConnectionStrings[connectionStringName];
            if (connString != null)
            {
                return connString.ConnectionString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => new Uri(x)).ToArray();
            }
            else
                throw new InvalidOperationException("Connection string \"" + connectionStringName + "\" was not found.");
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
    }
}
