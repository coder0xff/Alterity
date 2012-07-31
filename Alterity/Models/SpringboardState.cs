using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alterity.Models
{
    public class SpringboardState
    {
        public int Id { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SpringboardStateEntry> Entries { get; set; }
        //public String DocumentText { get; set; }

        public SpringboardState() { Entries = new List<SpringboardStateEntry>(); }

        public static SpringboardState Create()
        {
            return EntityMappingContext.Current.Springboards.Add(new SpringboardState());
        }

        public static SpringboardState Create(IEnumerable<EditOperation> activeEditOperations)
        {
            SpringboardState springboardState = Create();
            foreach (EditOperation editOperation in activeEditOperations)
                springboardState.Entries.Add(new SpringboardStateEntry(editOperation));
            return springboardState;
        }
    }
}
