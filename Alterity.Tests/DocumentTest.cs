using Alterity.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using System.Collections.Generic;

namespace Alterity.Tests
{
    
    
    /// <summary>
    ///This is a test class for DocumentTest and is intended
    ///to contain all DocumentTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DocumentTest
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
        ///A test for ProcessState
        ///</summary>
        [TestMethod()]
        public void ProcessStateTest()
        {
            Document document = new Document();
            document.VoteRatioThreshold = new float?(0.5f);
            List<ChangeSet> changeSets = new List<ChangeSet>();
            document.ChangeSets = changeSets;

            changeSets.Add(CreateTestChangeSet(document, 0, 0, "Hello!", true));
            changeSets.Add(CreateTestChangeSet(document, 1, 5, ", world", true));
            changeSets.Add(CreateTestChangeSet(document, 2, 6, "!!", false));
            changeSets.Add(CreateTestChangeSet(document, 3, 8, " Goodbye", true));
            changeSets.Add(CreateTestChangeSet(document, 4, 8, ", cruel world!", true));

            CreateTestSpringboardState(changeSets, 1, 0);
            CreateTestSpringboardState(changeSets, 2, 0);
            CreateTestSpringboardState(changeSets, 3, 0, 2);
            CreateTestSpringboardState(changeSets, 4, 0, 2);

            ICollection<EditOperation> sortedOperations = document.GetEditOperations();
            //ICollection<EditOperation> expected 
            ICollection<EditOperation> processOperations;
            processOperations = Document.ProcessState(sortedOperations);
            String actual = Document.Generate(processOperations);
            String expected = "Hello, world! Goodbye, cruel world!";
            Assert.AreEqual(expected, actual);
        }

        private static void CreateTestSpringboardState(List<ChangeSet> changeSets, int changeSetIndex, params int[] activeOperationIds)
        {
            List<SpringboardStateEntry> entries = (List<SpringboardStateEntry>)((List<ChangeSubset>)changeSets[changeSetIndex].ChangeSubsets)[0].SpringboardState.Entries;
            foreach (int activeOperationId in activeOperationIds)
            {
                SpringboardStateEntry entry = new SpringboardStateEntry();
                entry.EditOperationId = activeOperationId;
                entries.Add(entry);
            }
        }

        private static ChangeSet CreateTestChangeSet(Document document, int operationIndex, int insertPosition, String insertText, bool currentVoteState)
        {
            ChangeSet changeSet = new ChangeSet();
            changeSet.VoteBox = new VoteBox();
            changeSet.VoteBox.Votes = new List<VoteEntry>();
            changeSet.Id = operationIndex;
            if (!currentVoteState)
            {
                VoteEntry voteEntry = new VoteEntry();
                voteEntry.User = new UserData();
                voteEntry.WasUpvote = false;
                changeSet.VoteBox.Votes.Add(voteEntry);
                changeSet.VoteBox.Votes.Add(voteEntry);
                changeSet.VoteBox.Votes.Add(voteEntry);
            }
            List<ChangeSubset> changeSubsets = new List<ChangeSubset>();
            changeSet.ChangeSubsets = changeSubsets;
            ChangeSubset changeSubset = new ChangeSubset();
            changeSubset.Id = operationIndex;
            changeSubset.VoteBox = new VoteBox();
            changeSubset.VoteBox.Votes = new List<VoteEntry>();
            changeSubset.SpringboardState = new SpringboardState();
            changeSubset.SpringboardState.Entries = new List<SpringboardStateEntry>();
            changeSubsets.Add(changeSubset);
            List<EditOperation> operations = new List<EditOperation>();
            changeSubset.EditOperations = operations;
            InsertOperation insertOperation = new InsertOperation(insertPosition, insertText);
            insertOperation.Id = operationIndex;
            insertOperation.VoteBox = new VoteBox();
            insertOperation.VoteBox.Votes = new List<VoteEntry>();
            operations.Add(insertOperation);
            insertOperation.ChangeSubset = changeSubset;
            changeSubset.ChangeSet = changeSet;
            changeSet.Document = document;
            changeSet.User = new UserData();
            changeSet.User.UserName = operationIndex.ToString();
            return changeSet;
        }
    }
}
