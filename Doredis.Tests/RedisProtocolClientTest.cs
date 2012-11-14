using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace Doredis.Tests
{
    [TestClass]
    public class RedisProtocolClientTest
    {
        [TestMethod]
        public void ConnectDisconnectTest()
        {
            Doredis.RedisProtocolClient client = Doredis.RedisProtocolClient.Create("localhost");
            Assert.AreNotEqual(null, client);
            client.Dispose();
        }

        [TestMethod]
        public void SetGetTest()
        {
            using (Doredis.RedisProtocolClient client = Doredis.RedisProtocolClient.Create("localhost"))
            {
                client.SendCommand("set", new byte[][] { Encoding.UTF8.GetBytes("a"), Encoding.UTF8.GetBytes("Hello world!") });
                Assert.AreEqual("OK", client.ReadReply().Data.ToString());
                client.SendCommand("get", new byte[][] { Encoding.UTF8.GetBytes("a") });
                string reply = Encoding.UTF8.GetString((byte[])client.ReadReply().Data);
                Assert.AreEqual("Hello world!", reply);
            }
        }
    }
}
