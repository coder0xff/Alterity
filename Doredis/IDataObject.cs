using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doredis
{
    internal interface IDataObject
    {
        DataStoreShard GetDataStoreShard(string memberAbsolutePath);
        string GetMemberAbsolutePath(string name, bool ignoreCase);
        string GetAbsolutePath();
    }
}
