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

        protected InsertionHunk() { }

        Hunk[] ApplyTransformationResults(IntegerInterval[] intervals)
        {
            return new Hunk[] { new InsertionHunk(intervals[0].Left, Text) };
        }

        public override Hunk[] UndoPrior(Hunk hunk)
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            if (hunk is InsertionHunk)
            {
                return ApplyTransformationResults(ToIntegerInterval().DeleteTransformInsertion(hunk.ToIntegerInterval()));
            }
            else if (hunk is DeletionHunk)
            {
                return ApplyTransformationResults(ToIntegerInterval().InsertTransformInsertion(hunk.ToIntegerInterval()));
            }
            else if (hunk is NoOperationHunk)
            {
                throw new InvalidOperationException("NoOperationHunks should not be redone or undone");
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
                return ApplyTransformationResults(ToIntegerInterval().InsertTransformInsertion(hunk.ToIntegerInterval()));
            }
            else if (hunk is DeletionHunk)
            {
                return ApplyTransformationResults(ToIntegerInterval().DeleteTransformInsertion(hunk.ToIntegerInterval()));
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

        public override bool MergeSubsequent(ref Hunk other, out Hunk result)
        {
            if (other == null) throw new ArgumentNullException("other");
            if (other.StartIndex + Length >= StartIndex && other.StartIndex <= StartIndex + Length)
            {
                InsertionHunk otherAsInsertion = other as InsertionHunk;
                if (otherAsInsertion != null)
                {
                    if (other.StartIndex >= StartIndex)
                        other.Destroy();
                    other = null;
                    String resultText = Text.Insert(otherAsInsertion.StartIndex - StartIndex, otherAsInsertion.Text);
                    if (Id < 0)
                    {
                        result = new InsertionHunk(StartIndex, resultText);
                        return true;
                    }
                    else
                    {
                        EditOperation editOperation = EditOperation;
                        Destroy();
                        result = new InsertionHunk(StartIndex, resultText);
                        editOperation.Hunks.Add(result);
                        return true;
                    }
                }
                else
                {
                    DeletionHunk otherAsDeletion = other as DeletionHunk;
                    if (otherAsDeletion != null)
                    {
                        StringBuilder resultStringBuilder = new StringBuilder();
                        foreach (IntegerInterval remnantInterval in ToIntegerInterval().Subtract(otherAsDeletion.ToIntegerInterval()))
                        {
                            resultStringBuilder.Append(Text.Substring(remnantInterval.Left - StartIndex, remnantInterval.Length));
                        }
                        String resultText = resultStringBuilder.ToString();
                        int resultLength = resultText.Length;
                        int mutualAnnihilationLength = Length - resultLength;
                        int deletionRemainderLength = otherAsDeletion.Length - mutualAnnihilationLength;
                        EditOperation othersEditOperation = other.EditOperation;
                        other.Destroy();
                        if (deletionRemainderLength > 0)
                        {
                            if (other.Id < 0)
                                other = new DeletionHunk(otherAsDeletion.StartIndex, deletionRemainderLength);
                            else
                            {
                                other = new DeletionHunk(otherAsDeletion.StartIndex, deletionRemainderLength);
                                othersEditOperation.Hunks.Add(other);
                            }
                        }
                        else
                        {
                            other = null;
                        }

                        if (Id < 0)
                        {
                            result = new InsertionHunk(StartIndex, resultText);
                            return true;
                        }
                        else
                        {
                            EditOperation editOperation = EditOperation;
                            Destroy();
                            result = new InsertionHunk(StartIndex, resultText);
                            EditOperation.Hunks.Add(result);
                            return true;
                        }
                    }
                    else
                    {
                        result = this;
                        return false;
                    }
                }
            }
            else
            {
                result = this;
                return false;
            }
        }
    }
}