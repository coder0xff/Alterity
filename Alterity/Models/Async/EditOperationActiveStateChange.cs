using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models.Async
{
    public class EditOperationActiveStateChange
    {
        public int EditOperationId { get; set; }
        public Alterity.Models.EditOperationActivationState ActivationState { get; set; }
        public int SequenceNumber { get; set; }
    }
}