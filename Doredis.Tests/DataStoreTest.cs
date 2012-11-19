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
    }
}
