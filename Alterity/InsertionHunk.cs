using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity
{
    internal struct InsertionHunk : Hunk
    {
        public int StartIndex { get; private set; }
        public String Text { get; private set; }
        public int Length { get; private set; }

        public InsertionHunk(int startIndex, String text) : this()
        {
            StartIndex = startIndex;
            Text = text;
            Length = new System.Globalization.StringInfo(Text).LengthInTextElements;
        }

        public Hunk[] UndoPrior(Hunk hunk)
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            if (hunk is InsertionHunk)
            {
                InsertionHunk other = (InsertionHunk)hunk;
                int leftShift = Math.Max(0, Math.Min(StartIndex - other.StartIndex, other.Length));
                return new Hunk[] { new InsertionHunk(StartIndex - leftShift, Text) };
            }
            else if (hunk is DeletionHunk)
            {
                DeletionHunk other = (DeletionHunk)hunk;
                if (other.StartIndex <= StartIndex)
                {
                    return new Hunk[] { new InsertionHunk(StartIndex + other.Length, Text) };
                }
                else
                {
                    return new Hunk[] { this };
                }
            }
            else
            {
                throw new ArgumentException("Unrecognized hunk type", "hunk");
            }
        }

        public Hunk[] RedoPrior(Hunk hunk)
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            if (hunk is InsertionHunk)
            {
                InsertionHunk other = (InsertionHunk)hunk;
                if (other.StartIndex <= StartIndex)
                {
                    return new Hunk[] { new InsertionHunk(StartIndex + other.Length, Text) };
                }
                else
                {
                    return new Hunk[] { this };
                }
            }
            else if (hunk is DeletionHunk)
            {
                DeletionHunk other = (DeletionHunk)hunk;
                if (other.StartIndex < StartIndex + Length && other.StartIndex + other.Length > StartIndex)
                    throw new ApplicationException("Redoing a deletion hunk prior to an insertion hunk with overlapping ranges.");
                if (other.StartIndex < StartIndex)
                {
                    return new Hunk[] { new InsertionHunk(StartIndex - other.Length, Text) };
                }
                else
                {
                    return new Hunk[] { this };
                }
            }
            else
            {
                throw new ArgumentException("Unrecognized hunk type", "hunk");
            }
        }
    }
}