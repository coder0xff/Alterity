using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Doredis
{
    internal class RedisSubscribeClient
    {
        DataStoreShard client;
        Thread listenThread;

        internal RedisSubscribeClient(DataStoreShard client)
        {
            this.client = client;
            listenThread = new Thread(ListenLoop);
            listenThread.Start();
        }

        ~RedisSubscribeClient()
        {
            Dispose();
        }

        public void Dispose()
        {
            client.Dispose();
        }

        void ListenLoop()
        {
            while (true)
            {
                client.WaitForData(true);
                client.ReadReply();
            }
        }
    }
}
