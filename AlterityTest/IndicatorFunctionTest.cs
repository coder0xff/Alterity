using Alterity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Alterity.Indexing;

namespace AlterityTest
{
    
    
    /// <summary>
    ///This is a test class for IndicatorFunctionTest and is intended
    ///to contain all IndicatorFunctionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IndicatorFunctionTest
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
        ///A test for AnyTrue
        ///</summary>
        [TestMethod()]
        public void AnyTrueTest()
        {
            IndicatorFunction target = new IndicatorFunction(); // TODO: Initialize to an appropriate value

            Range hunk = new Range(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.AnyTrue(hunk);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
