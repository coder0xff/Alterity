﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models
{
    public class DeleteOperation : EditOperation
    {
        public DeleteOperation() { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DeleteOperation(int startIndex, int length)
        {
            Hunks.Add(new DeletionHunk(startIndex, length));
        }

        internal override EditOperation UndoPrior(EditOperation hunk)
        {
            return UndoPrior<DeleteOperation>(hunk);
        }

        internal override EditOperation RedoPrior(EditOperation hunk)
        {
            return RedoPrior<DeleteOperation>(hunk);
        }

        internal override EditOperation SubjoinSubsequent(EditOperation hunk)
        {
            return SubjoinSubsequent<DeleteOperation>(hunk);
        }
    }
}