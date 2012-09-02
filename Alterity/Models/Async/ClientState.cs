using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models.Async
{
    public class ClientState
    {
        public SortedSet<int> ActiveEditOperationIds { get; set; }
        public int MostRecentTransmissionSequenceNumber { get; set; }
    }
}