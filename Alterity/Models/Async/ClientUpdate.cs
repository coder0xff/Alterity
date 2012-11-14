using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models.Async
{
    public class ClientUpdate
    {
        public int ClientUpdateStamp { get; set; }
        public HunkDTO[] hunkDTOs { get; set; }
    }
}