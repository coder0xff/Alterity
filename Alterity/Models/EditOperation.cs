﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alterity.Models
{
    public enum EditOperationActivationState
    {
        Deactivated = 0,
        Activated = 1
    }
    
    public abstract class EditOperation
    {
        public int Id { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Hunk> Hunks { get; set; }
        public VoteBox VoteBox { get; set; }
        public int ChangeSubsetId { get; set; }
        [ForeignKey("ChangeSubsetId")]
        public ChangeSubset ChangeSubset { get; set; }
        public ChangeSet ChangeSet { get { return ChangeSubset.ChangeSet; } }
        public Document Document { get { return ChangeSubset.ChangeSet.Document; } }
        public User User { get { return ChangeSet.Owner; } }
        public bool IsClosed { get; protected set; }

        protected EditOperation()
        {
            VoteBox = VoteBox.Create();
            IsClosed = false;
        }

        public EditOperationActivationState GetVoteStatus()
        {
            ICollection<VoteEntry> allVotes = VoteBox.Inheret(VoteBox.Votes, ChangeSubset.VoteBox.Votes);
            allVotes = VoteBox.Inheret(allVotes, ChangeSet.VoteBox.Votes);
            float voteRatio = VoteBox.ComputeVoteRatio(allVotes);
            return voteRatio >= Document.VoteRatioThreshold ? EditOperationActivationState.Activated : EditOperationActivationState.Deactivated;
        }

        private T UndoPrior<T>(Hunk hunk) where T : EditOperation, new()
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            List<Hunk> resultingHunks = new List<Hunk>();
            foreach (Hunk unprocessedHunk in Hunks)
                resultingHunks.AddRange(unprocessedHunk.UndoPrior(hunk));
            T result = new T();
            result.Hunks = resultingHunks.ToArray();
            return result;
        }

        internal T UndoPrior<T>(EditOperation operation) where T: EditOperation, new()
        {
            if (operation == null) throw new ArgumentNullException("operation");
            T result = (T)this;
            foreach (Hunk hunk in operation.Hunks)
            {
                result = result.UndoPrior<T>(hunk);
            }
            return result;
        }

        //The return value is an instance of the implementing class
        internal abstract EditOperation UndoPrior(EditOperation hunk);

        private T RedoPrior<T>(Hunk hunk) where T : EditOperation, new()
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            List<Hunk> resultingHunks = new List<Hunk>();
            foreach (Hunk unprocessedHunk in Hunks)
                resultingHunks.AddRange(unprocessedHunk.RedoPrior(hunk));
            T result = new T();
            result.Hunks = resultingHunks.ToArray();
            return result;
        }

        internal T RedoPrior<T>(EditOperation operation) where T : EditOperation, new()
        {
            if (operation == null) throw new ArgumentNullException("operation");
            T result = (T)this;
            foreach (Hunk hunk in operation.Hunks)
            {
                result = result.RedoPrior<T>(hunk);
            }
            return result;
        }

        //The return value is an instance of the implementing class
        internal abstract EditOperation RedoPrior(EditOperation hunk);

        internal void Apply(System.Text.StringBuilder stringBuilder)
        {
            foreach (Hunk hunk in Hunks)
                hunk.Apply(stringBuilder);
        }

        public void Close() { IsClosed = true; }

        /// <summary>
        /// The hunk MUST have this operation's hunks as active priors
        /// </summary>
        /// <param name="hunk"></param>
        /// <returns></returns>
        public abstract bool MergeHunk(ref Hunk hunk);
    }
}