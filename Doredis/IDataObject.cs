using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doredis
{
    public interface IDataObject
    {
        DataStoreShard GetDataStore(string memberAbsolutePath);
        string GetMemberAbsolutePath(string name, bool ignoreCase);
        string GetAbsolutePath();
    }
}
