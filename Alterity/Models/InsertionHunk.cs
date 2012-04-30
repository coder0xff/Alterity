using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Alterity.Models
{
    [Table("InsertionHunks")]
    public class InsertionHunk : Hunk
    {
        public override int StartIndex { get; protected set; }
        public String Text { get; protected set; }
        public override int Length { get; protected set; }

        public InsertionHunk(int startIndex, String text)
        {
            StartIndex = startIndex;
            Text = text;
            Length = new System.Globalization.StringInfo(Text).LengthInTextElements;
        }

        Hunk[] ApplyTransformationResults(IntegerInterval[] intervals)
        {
            return new Hunk[] { new InsertionHunk(intervals[0].Position, Text) };
        }

        public override Hunk[] UndoPrior(Hunk hunk)
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            if (hunk is InsertionHunk)
            {
                return ApplyTransformationResults(ToInterval().DeleteTransformInsertion(hunk.ToInterval()));
            }
            else if (hunk is DeletionHunk)
            {
                return ApplyTransformationResults(ToInterval().InsertTransformInsertion(hunk.ToInterval()));
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

        public override Hunk[] RedoPrior(Hunk hunk)
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            if (hunk is InsertionHunk)
            {
                return ApplyTransformationResults(ToInterval().InsertTransformInsertion(hunk.ToInterval()));
            }
            else if (hunk is DeletionHunk)
            {
                return ApplyTransformationResults(ToInterval().DeleteTransformInsertion(hunk.ToInterval()));
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

        public override Hunk[] SubjoinSubsequent(Hunk hunk)
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            if (hunk is InsertionHunk)
            {
                return ApplyTransformationResults(ToInterval().InsertTransformInsertionSwappedPrecedence(hunk.ToInterval()));
            }
            else if (hunk is DeletionHunk)
            {
                return ApplyTransformationResults(ToInterval().DeleteTransformInsertion(hunk.ToInterval()));
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
            text.Insert(StartIndex, Text);
        }

        public override bool Equals(object obj)
        {
            var other = obj as InsertionHunk;
            if (other == null) return false;
            return base.Equals(obj) && other.Text == Text && other.StartIndex == StartIndex;
        }

        public override int GetHashCode()
        {
            return Utility.CombineHashCodes(base.GetHashCode(), StartIndex, Text);
        }

        public override string ToString()
        {
            string debugText = Text;
            if (Text.Length > 15)
                debugText = Text.Substring(0, 15) + "...";
            return StartIndex.ToString() + ", " + Length.ToString() + " (" + debugText + ")";
        }

        public new class ValueComparer : IComparer<InsertionHunk>
        {
            public int Compare(InsertionHunk x, InsertionHunk y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                int startIndexDifference;
                if ((startIndexDifference = x.StartIndex - y.StartIndex) != 0) return startIndexDifference;
                return String.CompareOrdinal(x.Text, y.Text);
            }
        }

        public new class IdComparer : IComparer<DeletionHunk>
        {
            public int Compare(DeletionHunk x, DeletionHunk y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                return x.Id - y.Id;
            }
        }

    }
}