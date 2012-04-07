using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models
{
    public class ChangeSubset
    {
        public int Id { get; set; }
        public VoteBox Votes { get; set; }
        public SpringboardState Springboard { get; set; }
    }
}