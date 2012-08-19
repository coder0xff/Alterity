using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Alterity.Models
{
    public class ChangeSet
    {
        public int Id { get; set; }
        public User Owner { get; set; }
        public VoteBox VoteBox { get; set; }
        public DateTime LastModified { get; set; }
        public Document Document { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChangeSubset> ChangeSubsets { get; set; }
        public bool IsClosed { get; set; }
        public ChangeSet()
        {
            LastModified = DateTime.Now;
            VoteBox = VoteBox.Create();
            IsClosed = false;
        }

        public IEnumerable<ChangeSubset> OpenChangeSubsets
        {
            get { return ChangeSubsets.Where((_) => _.IsClosed == false); }
        }
    }
}
