using Alterity.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using System.Text;

namespace Alterity.Tests
{
    
    
    /// <summary>
    ///This is a test class for DeletionHunkTest and is intended
    ///to contain all DeletionHunkTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DeletionHunkTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for DeletionHunk Constructor
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        public void DeletionHunkConstructorTest()
        {
            int startIndex = 50; // TODO: Initialize to an appropriate value
            int length = 99; // TODO: Initialize to an appropriate value
            DeletionHunk target = new DeletionHunk(startIndex, length);
            Assert.AreEqual(target.StartIndex, 50);
            Assert.AreEqual(target.Length, 99);
        }

        /// <summary>
        ///A test for Apply
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        public void ApplyTest()
        {
            int startIndex = 5; // TODO: Initialize to an appropriate value
            int length = 7; // TODO: Initialize to an appropriate value
            DeletionHunk target = new DeletionHunk(startIndex, length); // TODO: Initialize to an appropriate value
            StringBuilder text = new StringBuilder("Hello, world!"); // TODO: Initialize to an appropriate value
            target.Apply(text);
            Assert.AreEqual(text.ToString(), "Hello!");
        }

        /// <summary>
        ///A test for RedoPrior
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        public void RedoPriorDeletionTest0()
        {
            int startIndex = 10; // TODO: Initialize to an appropriate value
            int length = 10; // TODO: Initialize to an appropriate value
            DeletionHunk target = new DeletionHunk(startIndex, length); // TODO: Initialize to an appropriate value
            Hunk hunk = new DeletionHunk(5, 2); // TODO: Initialize to an appropriate value
            Hunk[] expected = new Hunk[] { new DeletionHunk(8, 10) }; // TODO: Initialize to an appropriate value
            Hunk[] actual;
            actual = target.RedoPrior(hunk);
            CollectionAssert.AreEqual(expected, actual, Hunk.ValueComparer);
        }

        [TestMethod()]
        public void RedoPriorDeletionTest1()
        {
            int startIndex = 10; // TODO: Initialize to an appropriate value
            int length = 10; // TODO: Initialize to an appropriate value
            DeletionHunk target = new DeletionHunk(startIndex, length); // TODO: Initialize to an appropriate value
            Hunk hunk = new DeletionHunk(5, 7); // TODO: Initialize to an appropriate value
            Hunk[] expected = new Hunk[] { new DeletionHunk(5, 8) }; // TODO: Initialize to an appropriate value
            Hunk[] actual;
            actual = target.RedoPrior(hunk);
            CollectionAssert.AreEqual(expected, actual, Hunk.ValueComparer);
        }

        [TestMethod()]
        public void RedoPriorDeletionTest2()
        {
            int startIndex = 10; // TODO: Initialize to an appropriate value
            int length = 10; // TODO: Initialize to an appropriate value
            DeletionHunk target = new DeletionHunk(startIndex, length); // TODO: Initialize to an appropriate value
            Hunk hunk = new DeletionHunk(12, 5); // TODO: Initialize to an appropriate value
            Hunk[] expected = new Hunk[] { new DeletionHunk(10, 5) }; // TODO: Initialize to an appropriate value
            Hunk[] actual;
            actual = target.RedoPrior(hunk);
            CollectionAssert.AreEqual(expected, actual, Hunk.ValueComparer);
        }

        [TestMethod()]
        public void RedoPriorDeletionTest3()
        {
            int startIndex = 10; // TODO: Initialize to an appropriate value
            int length = 10; // TODO: Initialize to an appropriate value
            DeletionHunk target = new DeletionHunk(startIndex, length); // TODO: Initialize to an appropriate value
            Hunk hunk = new DeletionHunk(12, 5); // TODO: Initialize to an appropriate value
            Hunk[] expected = new Hunk[] { new DeletionHunk(10, 5) }; // TODO: Initialize to an appropriate value
            Hunk[] actual;
            actual = target.RedoPrior(hunk);
            CollectionAssert.AreEqual(expected, actual, Hunk.ValueComparer);
        }

        /// <summary>
        ///A test for UndoPrior
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Users\\Brent\\Documents\\Visual Studio 2010\\Projects\\Alterity\\Alterity", "/")]
        [UrlToTest("http://localhost:16954/")]
        public void UndoPriorTest()
        {
            int startIndex = 0; // TODO: Initialize to an appropriate value
            int length = 0; // TODO: Initialize to an appropriate value
            DeletionHunk target = new DeletionHunk(startIndex, length); // TODO: Initialize to an appropriate value
            Hunk hunk = null; // TODO: Initialize to an appropriate value
            Hunk[] expected = null; // TODO: Initialize to an appropriate value
            Hunk[] actual;
            actual = target.UndoPrior(hunk);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Length
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Users\\Brent\\Documents\\Visual Studio 2010\\Projects\\Alterity\\Alterity", "/")]
        [UrlToTest("http://localhost:16954/")]
        [DeploymentItem("Alterity.dll")]
        public void LengthTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            DeletionHunk_Accessor target = new DeletionHunk_Accessor(param0); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.Length = expected;
            actual = target.Length;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for StartIndex
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Users\\Brent\\Documents\\Visual Studio 2010\\Projects\\Alterity\\Alterity", "/")]
        [UrlToTest("http://localhost:16954/")]
        [DeploymentItem("Alterity.dll")]
        public void StartIndexTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            DeletionHunk_Accessor target = new DeletionHunk_Accessor(param0); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.StartIndex = expected;
            actual = target.StartIndex;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
