using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity
{
    internal class InsertOperation : EditOperation
    {
        public InsertOperation() { }

        public InsertOperation(int startIndex, string text) :
            base(new Hunk[] { new InsertionHunk(startIndex, text) })
        {
        }

        internal override EditOperation UndoPrior(EditOperation hunk)
        {
            return UndoPrior<InsertOperation>(hunk);
        }
    }
}