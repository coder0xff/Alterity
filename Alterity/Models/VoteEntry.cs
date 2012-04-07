using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity.Models
{
    public class VoteEntry
    {
        public int Id { get; set; }
        public UserAccount User { get; set; }
        public bool WasUpvote { get; set; }
    }
}
