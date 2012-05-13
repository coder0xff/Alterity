using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models
{
    public class ChangeSubset
    {
        public int Id { get; set; }
        public VoteBox VoteBox { get; set; }
        public ChangeSet ChangeSet { get; set; }
        public SpringboardState SpringboardState { get; set; }
        public virtual ICollection<EditOperation> EditOperations { get; set; }
    }
}