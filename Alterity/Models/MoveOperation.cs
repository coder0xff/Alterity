using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models
{
    public class MoveOperation : EditOperation
    {
        public MoveOperation() { }

        internal override EditOperation UndoPrior(EditOperation hunk)
        {
            return UndoPrior<MoveOperation>(hunk);
        }

        internal override EditOperation RedoPrior(EditOperation hunk)
        {
            return RedoPrior<MoveOperation>(hunk);
        }

        public override bool MergeHunk(ref Hunk hunk)
        {
            return false;
        }
    }
}