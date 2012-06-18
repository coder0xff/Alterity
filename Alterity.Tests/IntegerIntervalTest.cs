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
            new IntegerInterval(4, 4),
            new IntegerInterval(4, 6),
            new IntegerInterval(4, 8),
            new IntegerInterval(6, 2),
            new IntegerInterval(6, 4),
            new IntegerInterval(6, 6),
            new IntegerInterval(7, 2),
            new IntegerInterval(8, 2),
            new IntegerInterval(8, 4),
            new IntegerInterval(10, 4),
            new IntegerInterval(11, 4),
        };

        class IntegetIntervalResult { public IntegerInterval[] intervals; }
        static IntegetIntervalResult[,] expectedValues;

        private TestContext testContextInstance;

        delegate IntegerInterval[] IntervalTransformMethod(IntegerInterval transformer);

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
                    if (startIndex == 0 && length == 0)
                    {
                        integerIntervalResult.intervals = new IntegerInterval[0];
                        break;
                    }
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
            Assert.AreEqual(startIndex, target.Left);
            Assert.AreEqual(length, target.Length);
        }

        private static void RunIntegerIntervalTestSet(int transformIndex, IntervalTransformMethod intervalTransformMethod)
        {
            for (int relationIndex = 0; relationIndex < 13; relationIndex++)
            {
                IntegerInterval transformer = YIntervals[relationIndex];
                IntegerInterval[] expected = expectedValues[transformIndex, relationIndex].intervals;
                IntegerInterval[] actual = intervalTransformMethod(transformer);
                CollectionAssert.AreEqual(expected, actual, "TransformIndex: " + transformIndex.ToString() + " RelationIndex: " + relationIndex.ToString());
            }
        }

        /// <summary>
        ///A test for DeleteTransformInsertion
        ///</summary>
        [TestMethod()]
        public void DeleteTransformInsertionTest()
        {
            int transformIndex = 3;
            IntervalTransformMethod intervalTransformMethod = new IntervalTransformMethod(XInterval.DeleteTransformInsertion);
            RunIntegerIntervalTestSet(transformIndex, intervalTransformMethod);
        }


        /// <summary>
        ///A test for DeleteTransformSelection
        ///</summary>
        [TestMethod()]
        public void DeleteTransformSelectionTest()
        {
            int transformIndex = 1;
            IntervalTransformMethod intervalTransformMethod = new IntervalTransformMethod(XInterval.DeleteTransformSelection);
            RunIntegerIntervalTestSet(transformIndex, intervalTransformMethod);
        }

//         /// <summary>
//         ///A test for InsertIntoInsertionSwappedPrecedence
//         ///</summary>
//         [TestMethod()]
//         public void InsertIntoInsertionSwappedPrecedenceTest()
//         {
//             int transformIndex = 4;
//             IntervalTransformMethod intervalTransformMethod = new IntervalTransformMethod(XInterval.InsertTransformInsertionSwappedPrecedence);
//             RunIntegerIntervalTestSet(transformIndex, intervalTransformMethod);
//         }

        /// <summary>
        ///A test for InsertTransformInsertion
        ///</summary>
        [TestMethod()]
        public void InsertTransformInsertionTest()
        {
            int transformIndex = 2;
            IntervalTransformMethod intervalTransformMethod = new IntervalTransformMethod(XInterval.InsertTransformInsertion);
            RunIntegerIntervalTestSet(transformIndex, intervalTransformMethod);
        }

        /// <summary>
        ///A test for InsertTransformSelection
        ///</summary>
        [TestMethod()]
        public void InsertTransformSelectionTest()
        {
            int transformIndex = 0;
            IntervalTransformMethod intervalTransformMethod = new IntervalTransformMethod(XInterval.InsertTransformSelection);
            RunIntegerIntervalTestSet(transformIndex, intervalTransformMethod);
        }
    }
}
