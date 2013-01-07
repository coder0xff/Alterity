using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doredis
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void DynamicObjectTest()
        {
            dynamic ds = new Doredis.DataStore(new System.Net.HostEndPoint[] { new System.Net.HostEndPoint("localhost", 6379) });
            ds.A = 5;
            int result = ds.A;
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void IncrementTest()
        {
            dynamic ds = new Doredis.DataStore(new System.Net.HostEndPoint[] { new System.Net.HostEndPoint("localhost", 6379) });
            ds.A = 5;
            long result = ds.A.Increment();
            Assert.AreEqual(6, result);
        }

        [TestMethod]
        public void MultiThreadLockTest()
        {
            dynamic ds = new Doredis.DataStore(new System.Net.HostEndPoint[] { new System.Net.HostEndPoint("localhost", 6379) });
            ds.A = 0;
            System.Threading.ThreadStart parallelCode = () =>
            {
                for (int count = 0; count < 1000; count++)
                    Doredis.Lock.On(ds.A, (Action)(() =>
                    {
                        int value = ds.A;
                        value += 2;
                        ds.A = value;
                        int check = ds.A;
                        Assert.AreEqual(value, check);
                        //System.Diagnostics.Debug.WriteLine("iteration " + count.ToString() + " on thread " + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
                    }));
            };
            int threadCount = 10;
            System.Threading.Thread[] workerThreads = new System.Threading.Thread[10];
            for (int threadIndex = 0; threadIndex < threadCount; threadIndex++)
            {
                System.Threading.Thread thread = new System.Threading.Thread(parallelCode);
                workerThreads[threadIndex] = thread;
                thread.Start();
            }
            for (int threadIndex = 0; threadIndex < threadCount; threadIndex++)
            {
                workerThreads[threadIndex].Join();
            }
            int finalValue = ds.A;
            Assert.AreEqual(threadCount * 1000 * 2, finalValue);
        }
   
        [TestMethod]
        public void ScriptingTest()
        {
            Doredis.DataStore ds = new Doredis.DataStore(new System.Net.HostEndPoint[] { new System.Net.HostEndPoint("localhost", 6379) });
            Func<IDataObject, int, int> script = ds.CreateScriptLambda<Func<IDataObject, int, int>>("return redis.call('get', KEYS[1]) + ARGV[1]");
            dynamic dds = ds;
            dds.A = 3;
            int result = script(dds.A, 5);
            Assert.AreEqual(8, result);
        }

        [TestMethod]
        public void ScriptingTest2()
        {
            Doredis.DataStore ds = new Doredis.DataStore(new System.Net.HostEndPoint[] { new System.Net.HostEndPoint("localhost", 6379) });
            Action<IDataObject, int, Action<RedisReply>> script = ds.CreateScriptLambda<Action<IDataObject, int, Action<RedisReply>>>("return redis.call('get', KEYS[1]) + ARGV[1]");
            dynamic dds = ds;
            dds.A = 3;
            int result = 0;
            script(dds.A, 5, (Action<RedisReply>)(_ => result = _.Expect<int>()));
            Assert.AreEqual(8, result);
        }
    }
}
