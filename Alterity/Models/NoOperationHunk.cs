using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Alterity.Models
{
    /// <summary>
    /// Used to store a selected range that was not modified (eg. for copy source)
    /// This class is nearly identical to DeletionHunk
    /// </summary>
    [Table("NoOperationHunk")]
    public class NoOperationHunk : Hunk
    {
        public int StartIndex { get; private set; }
        public int Length { get; private set; }

        public NoOperationHunk(int startIndex, int length)
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
                    return new Hunk[] { new NoOperationHunk(StartIndex - leftShiftCount, newLength) };
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
                        return new Hunk[] { new NoOperationHunk(StartIndex + asDeletion.Length, Length) };
                    }
                    else if (asDeletion.StartIndex < StartIndex + Length)
                    {
                        return new Hunk[] { new NoOperationHunk(StartIndex, asDeletion.StartIndex - StartIndex - 1),
                        new NoOperationHunk(asDeletion.StartIndex + asDeletion.Length, StartIndex + Length + asDeletion.Length - 1)};
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
                        return new Hunk[] { new NoOperationHunk(StartIndex, Length) };
                    }
                    else
                    {
                        throw new ArgumentException("Unrecognized hunk type", "hunk");
                    }
                }
            }            
        }

        public override Hunk[] RedoPrior(Hunk hunk)
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            InsertionHunk asInsertion = hunk as InsertionHunk;
            if (asInsertion != null)
            {
                if (asInsertion.StartIndex <= StartIndex)
                {
                    return new Hunk[] { new NoOperationHunk(StartIndex + asInsertion.Length, Length) };
                }
                else if (asInsertion.StartIndex < StartIndex + Length)
                {
                    return new Hunk[] { new NoOperationHunk(StartIndex, asInsertion.StartIndex - StartIndex - 1),
                        new NoOperationHunk(asInsertion.StartIndex + asInsertion.Length, StartIndex + Length + asInsertion.Length - 1)};
                }
                else
                {
                    return new Hunk[] { new NoOperationHunk(StartIndex, Length) };
                }
            }
            else
            {
                DeletionHunk asDeletion = hunk as DeletionHunk;
                if (asDeletion != null)
                {
                    int leftShiftCount = Math.Max(0, Math.Min(StartIndex - asDeletion.StartIndex, asDeletion.Length));
                    int lengthReduction = Math.Max(0, Math.Min(Math.Min(StartIndex + Length - asDeletion.StartIndex, asDeletion.StartIndex + asDeletion.Length - StartIndex), Length));
                    int newLength = Length - lengthReduction;
                    if (newLength > 0)
                    {
                        return new Hunk[] { new NoOperationHunk(StartIndex - leftShiftCount, newLength) };
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
                        return new Hunk[] { new NoOperationHunk(StartIndex, Length) };
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
            //Do nothing
        }
    }
}