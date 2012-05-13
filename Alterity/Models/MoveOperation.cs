using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models
{
    public class MoveOperation : EditOperation
    {
        public MoveOperation() { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MoveOperation(int sourceIndex, int destinationIndex, string text)
        {
            Hunks.Add(new DeletionHunk(sourceIndex, new System.Globalization.StringInfo(text).LengthInTextElements));
            Hunks.Add(new InsertionHunk(destinationIndex, text));
        }
        internal override EditOperation UndoPrior(EditOperation hunk)
        {
            return UndoPrior<MoveOperation>(hunk);
        }

        internal override EditOperation RedoPrior(EditOperation hunk)
        {
            return RedoPrior<MoveOperation>(hunk);
        }

        internal override EditOperation SubjoinSubsequent(EditOperation hunk)
        {
            return SubjoinSubsequent<MoveOperation>(hunk);
        }
    }
}