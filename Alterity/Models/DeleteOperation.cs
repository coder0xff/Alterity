using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models
{
    public class DeleteOperation : EditOperation
    {
        public DeleteOperation() { }

        internal override EditOperation UndoPrior(EditOperation hunk)
        {
            return UndoPrior<DeleteOperation>(hunk);
        }

        internal override EditOperation RedoPrior(EditOperation hunk)
        {
            return RedoPrior<DeleteOperation>(hunk);
        }

        public override bool MergeHunk(ref Hunk hunk)
        {
            if (IsClosed) throw new ApplicationException("Cannot merged into a closed hunk!");
            if (Hunks.Count != 1) throw new ApplicationException("Deletion hunks can only be merged if they have not been transformed!");
            Hunk current = (DeletionHunk)Hunks.First();
            Hunk result;
            if (current.MergeSubsequent(ref hunk, out result))
            {
                Hunks.Remove(current);
                current.Destroy();
                Hunks.Add(result);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}