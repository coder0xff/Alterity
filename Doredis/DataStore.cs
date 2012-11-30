﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace Doredis
{
    public class DataStore : DynamicObject, IDataObject
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

        DataStoreShard IDataObject.GetDataStoreShard(string memberAbsolutePath)
        {
            return shards.SelectShard(memberAbsolutePath.Split(new char[] { '.' })[0].GetHashCode());
        }

        string IDataObject.GetMemberAbsolutePath(string name, bool ignoreCase)
        {
            return ignoreCase ? name.ToLowerInvariant() : name;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = ((IDataObject)this).CreateMember(binder.Name, binder.IgnoreCase);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            ((IDataObject)this).AssignMember(binder.Name, binder.IgnoreCase, value);
            return true;
       }

        Scope CreateScope(string memberName)
        {
            return new Scope(memberName, ((IDataObject)this).GetDataStoreShard(memberName));
        }

        string IDataObject.GetAbsolutePath()
        {
            throw new InvalidOperationException("The top level data store object cannot be assigned to.");
        }

        public void CreateScriptTest()
        {
            throw new NotImplementedException();
        }

        System.Net.HostEndPoint IDataObject.EndPoint
        {
            get
            {
                throw new InvalidOperationException("The top level data store object does not have an endpoint.");
            }
        }
    }
}
