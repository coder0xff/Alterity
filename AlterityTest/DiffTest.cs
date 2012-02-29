using Alterity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlterityTest
{
    
    
    /// <summary>
    ///This is a test class for DiffTest and is intended
    ///to contain all DiffTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DiffTest
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
        ///A test for ComputeRelations
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Alterity.dll")]
        public void ComputeRelationsTest()
        {
            string left = string.Empty; // TODO: Initialize to an appropriate value
            string right = string.Empty; // TODO: Initialize to an appropriate value
            int minimumHunkLength = 15; // TODO: Initialize to an appropriate value
            List<MatchedRange> expected = new List<MatchedRange>(); // TODO: Initialize to an appropriate value
            List<MatchedRange> actual;
            actual = Diff_Accessor.ComputeRelations(left, right, minimumHunkLength);
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        /// <summary>
        ///A test for ComputeRelations
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Alterity.dll")]
        public void ComputeRelationsTest2()
        {
            string left = "Joseph threw the big red ball to Sally."; // TODO: Initialize to an appropriate value
            string right = "Joseph threw something to Sally. It was a big red ball."; // TODO: Initialize to an appropriate value
            int minimumHunkLength = 3; // TODO: Initialize to an appropriate value
            List<MatchedRange> expected = new List<MatchedRange>(); // TODO: Initialize to an appropriate value
            expected.Add(new MatchedRange(new Range[] { new Range(0, 12) }, new Range(0, 12)));
            expected.Add(new MatchedRange(new Range[] { new Range(29, 38) }, new Range(22, 31)));
            expected.Add(new MatchedRange(new Range[] { new Range(16, 28) }, new Range(41, 53)));
            List<MatchedRange> actual;
            actual = Diff_Accessor.ComputeRelations(left, right, minimumHunkLength);
            Assert.IsTrue(expected.SequenceEqual(actual));
        }
    }
}
