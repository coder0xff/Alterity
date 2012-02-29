using Alterity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AlterityTest
{
    
    
    /// <summary>
    ///This is a test class for StringIndexerTest and is intended
    ///to contain all StringIndexerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StringIndexerTest
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
        ///A test for BestMatchSearch
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Alterity.dll")]
        public void BestMatchSearchTest1()
        {
            String param0 = "This is a test of the emergency broadcast system. This is only a test."; // TODO: Initialize to an appropriate value
            StringIndexer_Accessor target = new StringIndexer_Accessor(param0); // TODO: Initialize to an appropriate value
            string matchString = "This annoying sound is for testing of the useless emergency broadcast system. This test is primarily just to annoy you.";
            int startingIndex = 0; // TODO: Initialize to an appropriate value
            int minimumLength = 1; // TODO: Initialize to an appropriate value
            MatchedRange expected = new MatchedRange(new Range[] { new Range(0, 4), new Range(50, 54) }, new Range(0, 4));
            MatchedRange actual;
            actual = target.BestMatchSearch(matchString.ToGraphemeArray(), startingIndex, minimumLength);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for BestMatchSearch
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Alterity.dll")]
        public void BestMatchSearchTest2()
        {
            String param0 = "This is a test of the emergency broadcast system. This is only a test."; // TODO: Initialize to an appropriate value
            StringIndexer_Accessor target = new StringIndexer_Accessor(param0); // TODO: Initialize to an appropriate value
            string matchString = "This annoying sound is for testing of the useless emergency broadcast system. This test is primarily just to annoy you.";
            int startingIndex = 5; // TODO: Initialize to an appropriate value
            int minimumLength = 1; // TODO: Initialize to an appropriate value
            MatchedRange expected = new MatchedRange(new Range[] { new Range(8, 8), new Range(35, 35), new Range(38, 38), new Range(63, 63) }, new Range(5, 5));
            MatchedRange actual;
            actual = target.BestMatchSearch(matchString.ToGraphemeArray(), startingIndex, minimumLength);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for BestMatchSearch
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Alterity.dll")]
        public void BestMatchSearchTest3()
        {
            String param0 = "This is a test of the emergency broadcast system. This is only a test."; // TODO: Initialize to an appropriate value
            StringIndexer_Accessor target = new StringIndexer_Accessor(param0); // TODO: Initialize to an appropriate value
            string matchString = "This annoying sound is for testing of the useless emergency broadcast system. This test is primarily just to annoy you.";
            int startingIndex = 5; // TODO: Initialize to an appropriate value
            int minimumLength = 3; // TODO: Initialize to an appropriate value
            MatchedRange expected = new MatchedRange(new Range[] { }, new Range(5, 5));
            MatchedRange actual;
            actual = target.BestMatchSearch(matchString.ToGraphemeArray(), startingIndex, minimumLength);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for BestMatchSearch
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Alterity.dll")]
        public void BestMatchSearchTest4()
        {
            String param0 = "This is a test of the emergency broadcast system. This is only a test."; // TODO: Initialize to an appropriate value
            StringIndexer_Accessor target = new StringIndexer_Accessor(param0); // TODO: Initialize to an appropriate value
            string matchString = "This annoying sound is for testing of the useless emergency broadcast system. This test is primarily just to annoy you. test. ";
            int startingIndex = 120; // TODO: Initialize to an appropriate value
            int minimumLength = 3; // TODO: Initialize to an appropriate value
            MatchedRange expected = new MatchedRange(new Range[] { new Range(65, 69) }, new Range(120, 124));
            MatchedRange actual;
            actual = target.BestMatchSearch(matchString.ToGraphemeArray(), startingIndex, minimumLength);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for BestMatchSearch
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Alterity.dll")]
        public void BestMatchSearchTest5()
        {
            String param0 = "This is a test of the emergency broadcast system. This is only a test."; // TODO: Initialize to an appropriate value
            StringIndexer_Accessor target = new StringIndexer_Accessor(param0); // TODO: Initialize to an appropriate value
            string matchString = "This annoying sound is for testing of the useless emergency broadcast system. This test is primarily just to annoy you. test.";
            int startingIndex = 120; // TODO: Initialize to an appropriate value
            int minimumLength = 3; // TODO: Initialize to an appropriate value
            MatchedRange expected = new MatchedRange(new Range[] { new Range(65, 69) }, new Range(120, 124));
            MatchedRange actual;
            actual = target.BestMatchSearch(matchString.ToGraphemeArray(), startingIndex, minimumLength);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for BestMatchSearch
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Alterity.dll")]
        public void BestMatchSearchTest6()
        {
            String param0 = "This is a test of the emergency broadcast system. This is only a test. Some more text."; // TODO: Initialize to an appropriate value
            StringIndexer_Accessor target = new StringIndexer_Accessor(param0); // TODO: Initialize to an appropriate value
            string matchString = "This annoying sound is for testing of the useless emergency broadcast system. This test is primarily just to annoy you. Some other text.";
            int startingIndex = 120; // TODO: Initialize to an appropriate value
            int minimumLength = 3; // TODO: Initialize to an appropriate value
            MatchedRange expected = new MatchedRange(new Range[] { new Range(71, 75) }, new Range(120, 124));
            MatchedRange actual;
            actual = target.BestMatchSearch(matchString.ToGraphemeArray(), startingIndex, minimumLength);
            Assert.AreEqual(expected, actual);
        }
    }
}
