using Alterity.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using System.Text;

namespace Alterity.Tests
{
    
    
    /// <summary>
    ///This is a test class for InsertionHunkTest and is intended
    ///to contain all InsertionHunkTest Unit Tests
    ///</summary>
    [TestClass()]
    public class InsertionHunkTest
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
        ///A test for InsertionHunk Constructor
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Users\\Brent\\Documents\\Visual Studio 2010\\Projects\\Alterity\\Alterity", "/")]
        [UrlToTest("http://localhost:16954/")]
        public void InsertionHunkConstructorTest()
        {
            int startIndex = 0; // TODO: Initialize to an appropriate value
            string text = string.Empty; // TODO: Initialize to an appropriate value
            InsertionHunk target = new InsertionHunk(startIndex, text);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Apply
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Users\\Brent\\Documents\\Visual Studio 2010\\Projects\\Alterity\\Alterity", "/")]
        [UrlToTest("http://localhost:16954/")]
        public void ApplyTest()
        {
            int startIndex = 0; // TODO: Initialize to an appropriate value
            string text = string.Empty; // TODO: Initialize to an appropriate value
            InsertionHunk target = new InsertionHunk(startIndex, text); // TODO: Initialize to an appropriate value
            StringBuilder text1 = null; // TODO: Initialize to an appropriate value
            target.Apply(text1);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for RedoPrior
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Users\\Brent\\Documents\\Visual Studio 2010\\Projects\\Alterity\\Alterity", "/")]
        [UrlToTest("http://localhost:16954/")]
        public void RedoPriorTest()
        {
            int startIndex = 0; // TODO: Initialize to an appropriate value
            string text = string.Empty; // TODO: Initialize to an appropriate value
            InsertionHunk target = new InsertionHunk(startIndex, text); // TODO: Initialize to an appropriate value
            Hunk hunk = null; // TODO: Initialize to an appropriate value
            Hunk[] expected = null; // TODO: Initialize to an appropriate value
            Hunk[] actual;
            actual = target.RedoPrior(hunk);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
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
            string text = string.Empty; // TODO: Initialize to an appropriate value
            InsertionHunk target = new InsertionHunk(startIndex, text); // TODO: Initialize to an appropriate value
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
            InsertionHunk_Accessor target = new InsertionHunk_Accessor(param0); // TODO: Initialize to an appropriate value
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
            InsertionHunk_Accessor target = new InsertionHunk_Accessor(param0); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.StartIndex = expected;
            actual = target.StartIndex;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Text
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Users\\Brent\\Documents\\Visual Studio 2010\\Projects\\Alterity\\Alterity", "/")]
        [UrlToTest("http://localhost:16954/")]
        [DeploymentItem("Alterity.dll")]
        public void TextTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            InsertionHunk_Accessor target = new InsertionHunk_Accessor(param0); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.Text = expected;
            actual = target.Text;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
