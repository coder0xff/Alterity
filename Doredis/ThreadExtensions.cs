using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Doredis
{
    public static class ThreadExtensions
    {
        public static void AddStopCallback(this Thread thread, Action handler)
        {
            new Thread(() => {
                thread.Join();
                handler();
            }).Start();

        }
    }
}
