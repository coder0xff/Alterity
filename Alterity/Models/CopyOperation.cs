using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models
{
    public class CopyOperation : EditOperation
    {
        public CopyOperation() { }

        internal override EditOperation UndoPrior(EditOperation hunk)
        {
            return UndoPrior<CopyOperation>(hunk);
        }

        internal override EditOperation RedoPrior(EditOperation hunk)
        {
            return RedoPrior<CopyOperation>(hunk);
        }

        public override bool MergeHunk(ref Hunk hunk)
        {
            return false;
        }
    }
}