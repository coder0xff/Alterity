using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Alterity.Models
{
    public class SpringboardStateEntry
    {
        [Key, Column(Order = 0)]
        public int SpringboardStateId { get; set; }
        [ForeignKey("SpringboardStateId")]
        public virtual SpringboardState SpringboardState { get; set; }

        [Key, Column(Order = 1)]
        public int EditOperationId { get; set; }
        [ForeignKey("EditOperationId")]
        public virtual EditOperation EditOperation { get; set; }

        public SpringboardStateEntry(EditOperation editOperation)
        {
            EditOperationId = editOperation.Id;
        }

        public SpringboardStateEntry(int editOperationId)
        {
            EditOperationId = editOperationId;
        }
    }
}
