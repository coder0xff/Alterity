using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Alterity.Models;

namespace Alterity.Models.Async
{
    public class HunkMergerThing
    {
        public virtual Document Document { get; set; }
        public virtual List<ClientState> ClientStates { get; set; }
        internal void ReceiveHunks(User user, int clientUpdateStamp, int serverUpdateStamp, Hunk[] hunks)
        {

        }
    }
}