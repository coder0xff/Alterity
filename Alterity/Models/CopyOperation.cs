using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models
{
    public class CopyOperation : EditOperation
    {
        public CopyOperation() { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CopyOperation(int sourceIndex, int destinationIndex, string text)
        {
            Hunks.Add(new DeletionHunk(sourceIndex, 0));
            Hunks.Add(new InsertionHunk(destinationIndex, text));
        }
        internal override EditOperation UndoPrior(EditOperation hunk)
        {
            return UndoPrior<CopyOperation>(hunk);
        }

        internal override EditOperation RedoPrior(EditOperation hunk)
        {
            return RedoPrior<CopyOperation>(hunk);
        }
    }
}