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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EditOperation> EditOperations { get; set; }
        public bool IsClosed { get; set; }

        public ChangeSubset(SpringboardState springboardState)
        {
            SpringboardState = springboardState;
            VoteBox = VoteBox.Create();
            IsClosed = false;
        }

        public ChangeSubset(IEnumerable<EditOperation> activeEditOperations)
            : this(SpringboardState.Create(activeEditOperations))
        {
        }

        public IEnumerable<EditOperation> OpenEditOperations
        {
            get
            {
                return EditOperations.Where((_) => _.IsClosed == false);
            }
        }
    }
}