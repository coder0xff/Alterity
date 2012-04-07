using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity.Models
{
    public class ChangeSet
    {
        public int Id { get; set; }
        public UserAccount User { get; set; }
        public VoteBox Votes { get; set; }
        public DateTime LastModified { get; set; }
    }
}
