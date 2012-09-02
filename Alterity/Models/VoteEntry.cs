using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alterity.Models
{
    public class VoteEntry
    {
        [Key, Column(Order = 0)]
        public int VoteBoxId { get; set; }
        [ForeignKey("VoteBoxId")]
        public virtual VoteBox VoteBox { get; set; }

        [Key, Column(Order = 1)]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public bool WasUpvote { get; set; }
    }
}
