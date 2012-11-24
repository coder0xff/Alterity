using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doredis
{
    internal static class IPathObjectExtensions
    {
        internal static object CreateMember(this IPathObject self, string name, bool ignoreCase)
        {
            string memberAbsolutePath = self.GetMemberAbsolutePath(name, ignoreCase);
            DataStoreShard dataStore = self.GetDataStoreShard(memberAbsolutePath);
            return new Scope(memberAbsolutePath, dataStore);
        }

        internal static void AssignMember(this IPathObject self, string name, bool ignoreCase, object value)
        {
            string memberAbsolutePath = self.GetMemberAbsolutePath(name, false);
            DataStoreShard dataStore = self.GetDataStoreShard(memberAbsolutePath);
            dataStore.Set(memberAbsolutePath, value);
        }
    }
}
