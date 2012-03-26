using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity
{
    internal class CopyOperation : EditOperation
    {
        public CopyOperation() { }

        public CopyOperation(int sourceIndex, int destinationIndex, string text) :
            base(new Hunk[] {
                new DeletionHunk(sourceIndex, 0),
                new InsertionHunk(destinationIndex, text)})
        {
        }
        internal override EditOperation UndoPrior(EditOperation hunk)
        {
            return UndoPrior<CopyOperation>(hunk);
        }
    }
}