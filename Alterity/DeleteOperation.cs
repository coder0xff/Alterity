using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity
{
    internal class DeleteOperation : EditOperation
    {
        public DeleteOperation() { }

        public DeleteOperation(int startIndex, int length) :
            base(new Hunk[] { new DeletionHunk(startIndex, length) })
        {
        }

        internal override EditOperation UndoPrior(EditOperation hunk)
        {
            return UndoPrior<DeleteOperation>(hunk);
        }
    }
}