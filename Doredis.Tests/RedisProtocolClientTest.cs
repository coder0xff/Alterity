using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Linq;
using System.Collections.Generic;

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
                Assert.AreEqual("Hello world!", Encoding.UTF8.GetString((byte[])client.ReadReply().Data));
            }
        }

        [TestMethod]
        public void TrimPushRangeTest()
        {
            using (Doredis.RedisProtocolClient client = Doredis.RedisProtocolClient.Create("localhost"))
            {
                client.SendCommand("ltrim", StringsToByteArrays(new List<string> { "A", "1", "0" }));
                Assert.AreEqual("OK", client.ReadReply().Data.ToString());

                List<string> wordList = new List<string> { "Aam", "Aardvark", "Aard-wolf" };

                List<string> paramList = new List<string>();
                paramList.Add("A");
                paramList.AddRange(wordList);
                client.SendCommand("lpush", StringsToByteArrays(paramList));
                Assert.AreEqual((Int64)3, client.ReadReply().Data);

                client.SendCommand("lrange", StringsToByteArrays(new List<string> { "A", "0", "2" }));
                Doredis.RedisReply[] result = (Doredis.RedisReply[])client.ReadReply().Data;
                string[] resultList = result.Select(x => Encoding.UTF8.GetString((byte[])x.Data)).ToArray();
                CollectionAssert.AreEquivalent(resultList, wordList);
            }
        }

        byte[][] StringsToByteArrays(List<string> strings)
        {
            return strings.Select(x => Encoding.UTF8.GetBytes(x)).ToArray();
        }
    }
}
