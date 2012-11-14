using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doredis
{
    public static class StreamExtensions
    {
        public static void WriteUtf8(this Stream self, params string[] text)
        {
            for (int textArrayIndex = 0; textArrayIndex < text.Length; textArrayIndex++)
            {
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(text[textArrayIndex]);
                self.Write(utf8Bytes, 0, utf8Bytes.Length);
            }
        }
    }
}
