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
        public int StartIndex { get; private set; }
        public String Text { get; private set; }
        public int Length { get; private set; }

        public InsertionHunk(int startIndex, String text)
        {
            StartIndex = startIndex;
            Text = text;
            Length = new System.Globalization.StringInfo(Text).LengthInTextElements;
        }

        public override Hunk[] UndoPrior(Hunk hunk)
        {
            if (hunk == null) throw new ArgumentNullException("hunk");
            InsertionHunk asInsertion = hunk as InsertionHunk;
            if (asInsertion != null)
            {
                int leftShift = Math.Max(0, Math.Min(StartIndex - asInsertion.StartIndex, asInsertion.Length));
                return new Hunk[] { new InsertionHunk(StartIndex - leftShift, Text) };
            }
            else
            {
                DeletionHunk asDeletion = hunk as DeletionHunk;
                if (asDeletion != null)
                {
                    if (asDeletion.StartIndex <= StartIndex)
                    {
                        return new Hunk[] { new InsertionHunk(StartIndex + asDeletion.Length, Text) };
                    }
                    else
                    {
                        return new Hunk[] { new InsertionHunk(StartIndex, Text) };
                    }
                }
                else
                {
                    if (hunk is NoOperationHunk)
                    {
                        return new Hunk[] { new InsertionHunk(StartIndex, Text) };
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
                    return new Hunk[] { new InsertionHunk(StartIndex + asInsertion.Length, Text) };
                }
                else
                {
                    return new Hunk[] { new InsertionHunk(StartIndex, Text) };
                }
            }
            else
            {
                DeletionHunk asDeletion = hunk as DeletionHunk;
                if (asDeletion != null)
                {
                    int leftShift = Math.Max(0, Math.Min(StartIndex - asDeletion.StartIndex, asDeletion.Length));
                    return new Hunk[] { new InsertionHunk(StartIndex - leftShift, Text) };
                }
                else
                {
                    if (hunk is NoOperationHunk)
                    {
                        return new Hunk[] { new InsertionHunk(StartIndex, Text) };
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
            text.Insert(StartIndex, Text);
        }
    }
}