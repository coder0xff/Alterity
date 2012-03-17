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


//         /// <summary>
//         ///A test for ComputeRelations
//         ///</summary>
//         [TestMethod()]
//         [DeploymentItem("Alterity.dll")]
//         public void ComputeRelationsTest()
//         {
//             string left = string.Empty; // TODO: Initialize to an appropriate value
//             string right = string.Empty; // TODO: Initialize to an appropriate value
//             int minimumHunkLength = 15; // TODO: Initialize to an appropriate value
//             List<RightRelationCandidateSet> expected = new List<RightRelationCandidateSet>(); // TODO: Initialize to an appropriate value
//             List<RightRelationCandidateSet> actual;
//             actual = Diff_Accessor.ComputeCandidateRelations(left, right, minimumHunkLength);
//             Assert.IsTrue(expected.SequenceEqual(actual));
//         }
// 
//         /// <summary>
//         ///A test for ComputeRelations
//         ///</summary>
//         [TestMethod()]
//         [DeploymentItem("Alterity.dll")]
//         public void ComputeRelationsTest2()
//         {
//             string left = "Joseph threw the big red ball to Sally."; // TODO: Initialize to an appropriate value
//             string right = "Joseph threw something to Sally. It was a big red ball."; // TODO: Initialize to an appropriate value
//             int minimumHunkLength = 3; // TODO: Initialize to an appropriate value
//             List<RightRelationCandidateSet> expected = new List<RightRelationCandidateSet>(); // TODO: Initialize to an appropriate value
//             expected.Add(new RightRelationCandidateSet(new Range[] { new Range(0, 12) }, new Range(0, 12)));
//             expected.Add(new RightRelationCandidateSet(new Range[] { new Range(29, 38) }, new Range(22, 31)));
//             expected.Add(new RightRelationCandidateSet(new Range[] { new Range(16, 28) }, new Range(41, 53)));
//             List<RightRelationCandidateSet> actual;
//             actual = Diff_Accessor.ComputeCandidateRelations(left, right, minimumHunkLength);
//             Assert.IsTrue(expected.SequenceEqual(actual));
//         }

        /// <summary>
        ///A test for Diff Constructor
        ///</summary>
        [TestMethod()]
        public void DiffConstructorTest()
        {
            string left = "John Jacob Jingleheimer Schmidt; his name is my name, too.";
            string right = "Dennis Rodman; his name is my name, too.";
            int minimumRelationLength = 15;
            Diff target = new Diff(left, right, minimumRelationLength);
            List<Relation> relatedRanges = new List<Relation>();
            relatedRanges.Add(new Relation(new Range(31, 57), new Range(13, 39)));
            List<Range> addedRanges = new List<Range>();
            addedRanges.Add(new Range(0, 12));
            List<Range> removedRanges = new List<Range>();
            removedRanges.Add(new Range(0, 30));
            Assert.IsTrue(relatedRanges.SequenceEqual(target.Relations));
            Assert.IsTrue(addedRanges.SequenceEqual(target.AddedRightRanges));
            Assert.IsTrue(removedRanges.SequenceEqual(target.DeletedLeftRanges));
        }
    }
}
