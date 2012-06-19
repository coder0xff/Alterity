using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models.Async
{
    public class ClientState
    {
//         public System.Text.StringBuilder DocumentText { get; set; }
        public SortedSet<int> ActiveEditOperationIds { get; set; }
        public int MostRecentTransmissionSequenceNumber { get; set; }
    }
}