using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models
{
    public class InsertOperation : EditOperation
    {
        public InsertOperation() { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public InsertOperation(int startIndex, string text)
        {
            Hunks = new List<Hunk>();
            Hunks.Add(new InsertionHunk(startIndex, text));
        }

        internal override EditOperation UndoPrior(EditOperation hunk)
        {
            return UndoPrior<InsertOperation>(hunk);
        }

        internal override EditOperation RedoPrior(EditOperation hunk)
        {
            return RedoPrior<InsertOperation>(hunk);
        }

        internal override EditOperation SubjoinSubsequent(EditOperation hunk)
        {
            return SubjoinSubsequent<InsertOperation>(hunk);
        }
    }
}