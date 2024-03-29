﻿namespace Doredis
{
    internal interface IDataObject
    {
        DataStoreShard GetDataStoreShard(string memberAbsolutePath);
        string GetMemberAbsolutePath(string name, bool ignoreCase);
        string GetAbsolutePath();
        System.Net.HostEndPoint EndPoint { get; }
    }

    internal static class DataObjectExtensions
    {
        internal static object CreateMember(this IDataObject self, string name, bool ignoreCase)
        {
            string memberAbsolutePath = self.GetMemberAbsolutePath(name, ignoreCase);
            DataStoreShard dataStore = self.GetDataStoreShard(memberAbsolutePath);
            return new Scope(memberAbsolutePath, dataStore);
        }

        internal static void AssignMember(this IDataObject self, string name, bool ignoreCase, object value)
        {
            string memberAbsolutePath = self.GetMemberAbsolutePath(name, false);
            DataStoreShard dataStore = self.GetDataStoreShard(memberAbsolutePath);
            dataStore.Set(memberAbsolutePath, value);
        }

        internal static DataStore GetDataStore(this IDataObject self)
        {
            return self.GetDataStoreShard("").Owner;
        }
    }
}
