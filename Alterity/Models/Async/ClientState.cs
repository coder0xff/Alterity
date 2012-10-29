using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models.Async
{
    public class ClientState
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public SortedSet<int> ActiveEditOperationIds { get; set; }
        public int MostRecentTransmissionSequenceNumber { get; set; }
    }
}