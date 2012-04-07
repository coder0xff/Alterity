using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity.Models
{
    public class SpringboardStateEntry
    {
        public int Id { get; set; }
        public EditOperation Edit { get; set; }
        public bool EditWasActive { get; set; }
    }
}
