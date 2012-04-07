using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Alterity
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
                    throw new ArgumentException("Unrecognized hunk type", "hunk");
                }
            }
        }

        public override Hunk[] RedoPrior(Hunk hunk)
        {
            throw new NotImplementedException();
        }
    }
}