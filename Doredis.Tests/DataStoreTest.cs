using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doredis.Tests
{
    [TestClass]
    public class DataStoreTest
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
                    }));
            };
            int threadCount = 1;
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
    }
}
