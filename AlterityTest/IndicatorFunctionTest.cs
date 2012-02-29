using Alterity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
            target.Add(new Range(5, 10));
            Range hunk = new Range(0, 4); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.AnyTrue(hunk);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for AnyTrue
        ///</summary>
        [TestMethod()]
        public void AnyTrueTest1()
        {
            IndicatorFunction target = new IndicatorFunction(); // TODO: Initialize to an appropriate value
            target.Add(new Range(5, 10));
            Range hunk = new Range(0, 5); // TODO: Initialize to an appropriate value
            bool expected = true; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.AnyTrue(hunk);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for AnyTrue
        ///</summary>
        [TestMethod()]
        public void AnyTrueTest2()
        {
            IndicatorFunction target = new IndicatorFunction(); // TODO: Initialize to an appropriate value
            target.Add(new Range(5, 10));
            target.Add(new Range(0, 3));
            target.Add(new Range(4, 5));
            target.Add(new Range(12, 15));
            target.Add(new Range(15, 20));
            target.Add(new Range(11, 11));
            Range hunk = new Range(4, 5); // TODO: Initialize to an appropriate value
            bool expected = true; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.AnyTrue(hunk);
            Assert.AreEqual(expected, actual);
        }
    }
}
