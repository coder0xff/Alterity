using System.IO;
using System.Text;

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
