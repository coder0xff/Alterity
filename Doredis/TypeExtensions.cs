using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doredis
{
    static class TypeExtensions
    {
        public static bool IsIntegral(this Type type)
        {
            if (type == typeof(sbyte)) return true;
            if (type == typeof(byte)) return true;
            if (type == typeof(short)) return true;
            if (type == typeof(ushort)) return true;
            if (type == typeof(int)) return true;
            if (type == typeof(uint)) return true;
            if (type == typeof(long)) return true;
            if (type == typeof(ulong)) return true;
            return false;
        }
    }
}
