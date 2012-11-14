using System;
using System.Net.Sockets;
using System.Threading;

namespace Doredis
{
    public static class TcpClientExtensions
    {
        public static bool Connect(this TcpClient self, String host, int port, int millisecondsTimeout = Timeout.Infinite)
        {
            ManualResetEvent blocker = new ManualResetEvent(false);
            IAsyncResult connecting = self.BeginConnect(host, port, result => { blocker.Set(); }, null);
            if (blocker.WaitOne(millisecondsTimeout))
                return true;
            self.Close();
            return false;
        }
    }
}
