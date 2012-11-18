using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Doredis
{
    class SerializationProvider
    {
        private static BinaryFormatter formatter = new BinaryFormatter();

        public static object Deserialize(byte[] data)
        {
            var tempStream = new System.IO.MemoryStream();
            tempStream.Write(data, 0, data.Length);
            tempStream.Seek(0, System.IO.SeekOrigin.Begin);
            return formatter.Deserialize(tempStream);  
        }

        public static byte[] Serialize(object value)
        {
            var tempStream = new System.IO.MemoryStream();
            formatter.Serialize(tempStream, value);
            return tempStream.ToArray();
        }
    }
}
