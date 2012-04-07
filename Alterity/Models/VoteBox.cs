using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity.Models
{
    public class VoteBox
    {
        public int Id { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ICollection<VoteEntry> Votes { get; set; }
    }
}
