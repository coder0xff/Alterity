using Alterity.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using System.Text;

namespace Alterity.Tests
{
    
    
    /// <summary>
    ///This is a test class for NoOperationHunkTest and is intended
    ///to contain all NoOperationHunkTest Unit Tests
    ///</summary>
    [TestClass()]
    public class NoOperationHunkTest
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
        ///A test for NoOperationHunk Constructor
        ///</summary>
        [TestMethod()]
        public void NoOperationHunkConstructorTest()
        {
            int startIndex = 0; // TODO: Initialize to an appropriate value
            int length = 0; // TODO: Initialize to an appropriate value
            NoOperationHunk target = new NoOperationHunk(startIndex, length);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Apply
        ///</summary>



        [TestMethod()]



        public void ApplyTest()
        {
            int startIndex = 0; // TODO: Initialize to an appropriate value
            int length = 0; // TODO: Initialize to an appropriate value
            NoOperationHunk target = new NoOperationHunk(startIndex, length); // TODO: Initialize to an appropriate value
            StringBuilder text = null; // TODO: Initialize to an appropriate value
            target.Apply(text);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for RedoPrior
        ///</summary>



        [TestMethod()]



        public void RedoPriorTest()
        {
            int startIndex = 0; // TODO: Initialize to an appropriate value
            int length = 0; // TODO: Initialize to an appropriate value
            NoOperationHunk target = new NoOperationHunk(startIndex, length); // TODO: Initialize to an appropriate value
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



        [TestMethod()]



        public void UndoPriorTest()
        {
            int startIndex = 0; // TODO: Initialize to an appropriate value
            int length = 0; // TODO: Initialize to an appropriate value
            NoOperationHunk target = new NoOperationHunk(startIndex, length); // TODO: Initialize to an appropriate value
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
        [TestMethod()]
        public void LengthTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            NoOperationHunk_Accessor target = new NoOperationHunk_Accessor(param0); // TODO: Initialize to an appropriate value
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
        [TestMethod()]
        public void StartIndexTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            NoOperationHunk_Accessor target = new NoOperationHunk_Accessor(param0); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            target.StartIndex = expected;
            actual = target.StartIndex;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
