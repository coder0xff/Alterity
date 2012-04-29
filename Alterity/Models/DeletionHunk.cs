﻿using System;
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

        public int StartIndex { get; private set; }
        public int Length { get; private set; }

        public DeletionHunk(int startIndex, int length)
        {
            StartIndex = startIndex;
            Length = length;
        }

        public override Hunk[] UndoPrior(Hunk hunk)
        {
            if (hunk == null) throw new ArgumentNullException("hunk");

            InsertionHunk asInsertion = hunk as InsertionHunk;
            if (asInsertion != null)
            {
                int leftShiftCount = Math.Max(0, Math.Min(StartIndex - asInsertion.StartIndex, asInsertion.Length));
                int lengthReduction = Math.Max(0, Math.Min(Math.Min(StartIndex + Length - asInsertion.StartIndex, asInsertion.StartIndex + asInsertion.Length - StartIndex), Length));
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
                DeletionHunk asDeletion = hunk as DeletionHunk;
                if (asDeletion != null)
                {
                    if (asDeletion.StartIndex <= StartIndex)
                    {
                        return new Hunk[] { new DeletionHunk(StartIndex + asDeletion.Length, Length) };
                    }
                    else if (asDeletion.StartIndex < StartIndex + Length)
                    {
                        return new Hunk[] { new DeletionHunk(StartIndex, asDeletion.StartIndex - StartIndex - 1),
                        new DeletionHunk(asDeletion.StartIndex + asDeletion.Length, StartIndex + Length + asDeletion.Length - 1)};
                    }
                    else
                    {
                        return new Hunk[] { this };
                    }
                }
                else
                {
                    if (hunk is NoOperationHunk)
                    {
                        return new Hunk[] { new DeletionHunk(StartIndex, Length) };
                    }
                    else
                    {
                        throw new ArgumentException("Unrecognized hunk type", "hunk");
                    }
                }
            }
        }

        // this method assumes that the hunk being redone was NOT part of the
        // springboard state. If it were part of the spring board state, then
        // the hunk stored in the database should just be retrieved
        public override Hunk[] RedoPrior(Hunk hunk)
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            InsertionHunk asInsertion = hunk as InsertionHunk;
            if (asInsertion != null)
            {
                if (asInsertion.StartIndex <= StartIndex)
                {
                    return new Hunk[] { new DeletionHunk(StartIndex + asInsertion.Length, Length) };
                }
                else if (asInsertion.StartIndex < StartIndex + Length)
                {
                    return new Hunk[] { new DeletionHunk(StartIndex, asInsertion.StartIndex - StartIndex - 1),
                        new DeletionHunk(asInsertion.StartIndex + asInsertion.Length, StartIndex + Length + asInsertion.Length - 1)};
                }
                else
                {
                    return new Hunk[] { new DeletionHunk(StartIndex, Length) };
                }
            }
            else
            {
                DeletionHunk asDeletion = hunk as DeletionHunk;
                if (asDeletion != null)
                {
                    int leftShiftCount = Math.Max(0, Math.Min(StartIndex - asDeletion.StartIndex, asDeletion.Length));
                    int commonRangeEnd = Math.Min(StartIndex + Length - 1, asDeletion.StartIndex + asDeletion.Length - 1);
                    int commonRangeStart = Math.Max(StartIndex, asDeletion.StartIndex);
                    int lengthReduction = Math.Max(0, commonRangeEnd - commonRangeStart + 1);
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
                    if (hunk is NoOperationHunk)
                    {
                        return new Hunk[] { new DeletionHunk(StartIndex, Length) };
                    }
                    else
                    {
                        throw new ArgumentException("Unrecognized hunk type", "hunk");
                    }
                }
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
    }
}