using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Alterity.Models
{
    [Table("DeletionHunks")]
    public class DeletionHunk : Hunk
    {

        public override int StartIndex { get; protected set; }
        public override int Length { get; protected set; }

        public DeletionHunk(int startIndex, int length)
        {
            StartIndex = startIndex;
            Length = length;
        }

        Hunk[] ApplyTransformationResults(IntegerInterval[] intervals)
        {
            DeletionHunk[] results = intervals.Select(x => new DeletionHunk(x.Left, x.Length)).ToArray();
            for (int transformeeIndex = 1; transformeeIndex < results.Length; transformeeIndex++)
            {
                for (int transformerIndex = 0; transformerIndex < transformeeIndex; transformerIndex++)
                {
                    results[transformeeIndex] = (DeletionHunk)results[transformeeIndex].RedoPrior(results[transformerIndex])[0];
                }
            }
            return results;
        }

        public override Hunk[] UndoPrior(Hunk hunk)
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            if (hunk is InsertionHunk)
            {
                return ApplyTransformationResults(ToIntegerInterval().DeleteTransformSelection(hunk.ToIntegerInterval()));
            }
            else if (hunk is DeletionHunk)
            {
                return ApplyTransformationResults(ToIntegerInterval().InsertTransformSelection(hunk.ToIntegerInterval()));
            }
            else if (hunk is NoOperationHunk)
            {
                throw new InvalidOperationException("NoOperationHunks should not be redone, undone, or subjoined.");
            }
            else
            {
                throw new ArgumentException("Unrecognized hunk type", "hunk");
            }
        }

        // this method assumes that the hunk being redone was NOT part of the
        // springboard state. If it were part of the spring board state, then
        // the hunk stored in the database should just be retrieved
        public override Hunk[] RedoPrior(Hunk hunk)
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            if (hunk is InsertionHunk)
            {
                return ApplyTransformationResults(ToIntegerInterval().InsertTransformSelection(hunk.ToIntegerInterval()));
            }
            else if (hunk is DeletionHunk)
            {
                return ApplyTransformationResults(ToIntegerInterval().DeleteTransformSelection(hunk.ToIntegerInterval()));
            }
            else if (hunk is NoOperationHunk)
            {
                throw new InvalidOperationException("NoOperationHunks should not be redone, undone, or subjoined.");
            }
            else
            {
                throw new ArgumentException("Unrecognized hunk type", "hunk");
            }
        }

        public override void Apply(StringBuilder text)
        {
            if (text == null) throw new ArgumentNullException("text");
            text.Remove(StartIndex, Length);
        }

        public override bool Equals(object obj)
        {
            var other = obj as DeletionHunk;
            if (other == null) return false;
            return other.Id == Id && other.Length == Length && other.StartIndex == StartIndex;
        }

        public override int GetHashCode()
        {
            return Utility.CombineHashCodes(base.GetHashCode(), StartIndex, Length);
        }

        public override string ToString()
        {
            return StartIndex.ToString() + ", " + Length.ToString();
        }

        public class ValueComparer : IComparer<DeletionHunk>
        {
            public int Compare(DeletionHunk x, DeletionHunk y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                int lengthDifference;
                if ((lengthDifference = x.Length - y.Length) != 0) return lengthDifference;
                return x.StartIndex - y.StartIndex;
            }
        }

        public class IdComparer : IComparer<DeletionHunk>
        {
            public int Compare(DeletionHunk x, DeletionHunk y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                return x.Id - y.Id;
            }
        }

        public override Hunk MergeSubsequent(ref Hunk other)
        {
            if (other == null) throw new ArgumentNullException("other");
            InsertionHunk otherAsInsertion = other as InsertionHunk;
            if (otherAsInsertion != null)
            {
                return this;
            }
            else
            {
                DeletionHunk otherAsDeletion = other as DeletionHunk;
                if (otherAsDeletion != null)
                {
                    IntegerInterval otherInterval = otherAsDeletion.ToIntegerInterval();
                    if (otherInterval.Right == StartIndex - 1)
                    {
                        other = null;
                        return new DeletionHunk(otherAsDeletion.StartIndex, Length + otherAsDeletion.Length);
                    }
                    else if (otherInterval.Left == StartIndex)
                    {
                        other = null;
                        return new DeletionHunk(StartIndex, Length + otherAsDeletion.Length);
                    }
                    else
                    {
                        return this;
                    }
                }
                else
                {
                    return this;
                }
            }
        }
    }
}