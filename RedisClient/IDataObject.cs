using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis
{
    interface IDataObject
    {
        abstract ServiceStack.Redis.RedisClient GetDataStore(string memberAbsolutePath);
        abstract string GetMemberAbsolutePath(string name, bool ignoreCase);
    }
}
