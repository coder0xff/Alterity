using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity
{
    internal struct DeletionHunk : Hunk
    {
        public int StartIndex { get; private set; }
        public int Length { get; private set; }

        public DeletionHunk(int startIndex, int length) : this()
        {
            StartIndex = startIndex;
            Length = length;
        }

        public Hunk[] UndoPrior(Hunk hunk)
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            if (hunk is InsertionHunk)
            {
                InsertionHunk other = (InsertionHunk)hunk;
                int leftShiftCount = Math.Max(0, Math.Min(StartIndex - other.StartIndex, other.Length));
                int lengthReduction = Math.Max(0, Math.Min(Math.Min(StartIndex + Length - other.StartIndex, other.StartIndex + other.Length - StartIndex), Length));
                int newLength = Length - lengthReduction;
                if (newLength > 0)
                {
                    return new Hunk[] { new DeletionHunk(StartIndex - leftShiftCount, newLength) };
                }
                else
                {
                    return new Hunk[] {};
                }
            }
            else if (hunk is DeletionHunk)
            {
                DeletionHunk other = (DeletionHunk)hunk;
                if (other.StartIndex <= StartIndex)
                {
                    return new Hunk[] { new DeletionHunk(StartIndex + other.Length, Length) };
                }
                else if (other.StartIndex < StartIndex + Length)
                {
                    return new Hunk[] { new DeletionHunk(StartIndex, other.StartIndex - StartIndex - 1),
                        new DeletionHunk(other.StartIndex + other.Length, StartIndex + Length + other.Length - 1)};
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

        // this method assumes that the hunk being redone was NOT part of the
        // springboard state. If it were part of the spring board state, then
        // the hunk stored in the database should just be retrieved
        public Hunk[] RedoPrior(Hunk hunk)
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            if (hunk is InsertionHunk)
            {
                InsertionHunk other = (InsertionHunk)hunk;
                if (other.StartIndex <= StartIndex)
                {
                    return new Hunk[] { new DeletionHunk(StartIndex + other.Length, Length) };
                }
                else if (other.StartIndex < StartIndex + Length)
                {
                    return new Hunk[] { new DeletionHunk(StartIndex, other.StartIndex - StartIndex - 1),
                        new DeletionHunk(other.StartIndex + other.Length, StartIndex + Length + other.Length - 1)};
                }
                else
                {
                    return new Hunk[] { this };
                }

            }
            else if (hunk is DeletionHunk)
            {
                DeletionHunk other = (DeletionHunk)hunk;
                int leftShiftCount = Math.Max(0, Math.Min(StartIndex - other.StartIndex, other.Length));
                int lengthReduction = Math.Max(0, Math.Min(Math.Min(StartIndex + Length - other.StartIndex, other.StartIndex + other.Length - StartIndex), Length));
                int newLength = Length - lengthReduction;
                if (newLength > 0)
                {
                    return new Hunk[] { new DeletionHunk(StartIndex - leftShiftCount, newLength) };
                }
                else
                {
                    return new Hunk[] { };
                }
            }
            else
            {
                throw new ArgumentException("Unrecognized hunk type", "hunk");
            }
        }
    }
}