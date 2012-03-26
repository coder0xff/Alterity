using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity
{
    internal class MoveOperation : EditOperation
    {
        public MoveOperation() { }

        public MoveOperation(int sourceIndex, int destinationIndex, string text) :
            base(new Hunk[] {
                new DeletionHunk(sourceIndex, new System.Globalization.StringInfo(text).LengthInTextElements),
                new InsertionHunk(destinationIndex, text)})
        {
        }
        internal override EditOperation UndoPrior(EditOperation hunk)
        {
            return UndoPrior<MoveOperation>(hunk);
        }
    }
}