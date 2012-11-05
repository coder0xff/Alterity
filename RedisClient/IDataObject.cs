using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redis
{
    interface IDataObject
    {
        ServiceStack.Redis.RedisClient GetDataStore(string memberAbsolutePath);
        string GetMemberAbsolutePath(string name, bool ignoreCase);
    }
}
