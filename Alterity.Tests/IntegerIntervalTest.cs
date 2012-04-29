using Alterity.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;

namespace Alterity.Tests
{
    
    
    /// <summary>
    ///This is a test class for IntegerIntervalTest and is intended
    ///to contain all IntegerIntervalTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IntegerIntervalTest
    {
        static IntegerInterval XInterval = new IntegerInterval(6, 4);
        static IntegerInterval[] YIntervals = new IntegerInterval[] {
            new IntegerInterval(1, 4),
            new IntegerInterval(2, 4),
            new IntegerInterval(1, 4),
            new IntegerInterval(1, 6),
            new IntegerInterval(1, 8),
            new IntegerInterval(6, 2),
            new IntegerInterval(6, 4),
            new IntegerInterval(6, 6),
            new IntegerInterval(7, 2),
            new IntegerInterval(8, 2),
            new IntegerInterval(8, 4),
            new IntegerInterval(10, 4),
            new IntegerInterval(11, 4),
        };

        struct IntegetIntervalResult { public IntegerInterval[] intervals; }
        static IntegetIntervalResult[,] expectedValues;

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
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            expectedValues = new IntegetIntervalResult[5, 13];
            String expectedValuesText = Alterity.Tests.Properties.Resources.IntegerInterval_Test_Expected_Values;            
            int transformIndex = 0;
            int transformRelationIndex = 0;
            int lineNumber = 0;
            foreach (String line in expectedValuesText.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                lineNumber++;
                String trimmedLine = line.Trim();
                if (trimmedLine.Length == 0 || trimmedLine.StartsWith("#")) continue;
                String[] numbers = trimmedLine.Split(new char[] { ' ', '\t' });
                var integerIntervalResult = new IntegetIntervalResult();
                integerIntervalResult.intervals = new IntegerInterval[numbers.Length / 2];
                expectedValues[transformIndex, transformRelationIndex] = integerIntervalResult;
                for (int numberIndex = 0; numberIndex < numbers.Length; numberIndex += 2)
                {
                    int startIndex = System.Convert.ToInt32(numbers[numberIndex]);
                    int length = System.Convert.ToInt32(numbers[numberIndex + 1]);
                    integerIntervalResult.intervals[numberIndex / 2] = new IntegerInterval(startIndex, length);
                }
                transformRelationIndex++;
                if (transformRelationIndex == 13)
                {
                    transformRelationIndex = 0;
                    transformIndex++;
                }
            }
        }
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
        ///A test for IntegerInterval Constructor
        ///</summary>
        [TestMethod()]
        public void IntegerIntervalConstructorTest()
        {
            int startIndex = 6; // TODO: Initialize to an appropriate value
            int length = 4; // TODO: Initialize to an appropriate value
            IntegerInterval target = new IntegerInterval(startIndex, length);
            Assert.AreEqual(startIndex, target.Position);
            Assert.AreEqual(length, target.Length);
        }

        /// <summary>
        ///A test for DeleteTransformInsertion
        ///</summary>
        [TestMethod()]
        public void DeleteTransformInsertionTest()
        {
            IntegerInterval target = new IntegerInterval(); // TODO: Initialize to an appropriate value
            IntegerInterval asDeletion = new IntegerInterval(); // TODO: Initialize to an appropriate value
            IntegerInterval[] expected = null; // TODO: Initialize to an appropriate value
            IntegerInterval[] actual;
            actual = target.DeleteTransformInsertion(asDeletion);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for DeleteTransformSelection
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Users\\Brent\\Documents\\Visual Studio 2010\\Projects\\Alterity\\Alterity", "/")]
        [UrlToTest("http://localhost:16954/")]
        public void DeleteTransformSelectionTest()
        {
            IntegerInterval target = new IntegerInterval(); // TODO: Initialize to an appropriate value
            IntegerInterval asInsertion = new IntegerInterval(); // TODO: Initialize to an appropriate value
            IntegerInterval[] expected = null; // TODO: Initialize to an appropriate value
            IntegerInterval[] actual;
            actual = target.DeleteTransformSelection(asInsertion);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for InsertIntoInsertionSwappedPrecedence
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Users\\Brent\\Documents\\Visual Studio 2010\\Projects\\Alterity\\Alterity", "/")]
        [UrlToTest("http://localhost:16954/")]
        public void InsertIntoInsertionSwappedPrecedenceTest()
        {
            IntegerInterval target = new IntegerInterval(); // TODO: Initialize to an appropriate value
            IntegerInterval asInsertion = new IntegerInterval(); // TODO: Initialize to an appropriate value
            IntegerInterval[] expected = null; // TODO: Initialize to an appropriate value
            IntegerInterval[] actual;
            actual = target.InsertIntoInsertionSwappedPrecedence(asInsertion);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for InsertTransformInsertion
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Users\\Brent\\Documents\\Visual Studio 2010\\Projects\\Alterity\\Alterity", "/")]
        [UrlToTest("http://localhost:16954/")]
        public void InsertTransformInsertionTest()
        {
            IntegerInterval target = new IntegerInterval(); // TODO: Initialize to an appropriate value
            IntegerInterval asInsertion = new IntegerInterval(); // TODO: Initialize to an appropriate value
            IntegerInterval[] expected = null; // TODO: Initialize to an appropriate value
            IntegerInterval[] actual;
            actual = target.InsertTransformInsertion(asInsertion);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for InsertTransformSelection
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        [HostType("ASP.NET")]
        [AspNetDevelopmentServerHost("C:\\Users\\Brent\\Documents\\Visual Studio 2010\\Projects\\Alterity\\Alterity", "/")]
        [UrlToTest("http://localhost:16954/")]
        public void InsertTransformSelectionTest()
        {
            IntegerInterval target = new IntegerInterval(); // TODO: Initialize to an appropriate value
            IntegerInterval Tranformer = new IntegerInterval(); // TODO: Initialize to an appropriate value
            IntegerInterval[] expected = null; // TODO: Initialize to an appropriate value
            IntegerInterval[] actual;
            actual = target.InsertTransformSelection(Tranformer);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
