using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity.Models
{
    public class ChangeSet
    {
        public int Id { get; set; }
        public UserData User { get; set; }
        public VoteBox VoteBox { get; set; }
        public DateTime LastModified { get; set; }
        public Document Document { get; set; }
        public virtual ICollection<ChangeSubset> ChangeSubsets { get; set; }
    }
}
