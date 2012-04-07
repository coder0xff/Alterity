using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity.Models
{
    public class SpringboardState
    {
        public int Id { get; set; }
        public virtual ICollection<SpringboardStateEntry> Entries { get; set; }
        public String DocumentText { get; set; }
    }
}
