using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis
{
    class SortedSet : IDataObject
    {
        ServiceStack.Redis.RedisClient dataStore;
        string absolutePath;

        public ServiceStack.Redis.RedisClient GetDataStore(string memberAbsolutePath)
        {
            return dataStore;
        }

        public string GetMemberAbsolutePath(string name, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        public string GetAbsolutePath()
        {
            throw new NotImplementedException();
        }
    }
}
