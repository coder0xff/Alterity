using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models.Async
{
    public class ClientStateMachine
    {
        ClientState clientState;
        Document document;
        void AdvanceState(EditOperationActiveStateChange stateChange)
        {
            lock (clientState)
            {
                if (stateChange.ActivationState == EditOperation.ActivationState.Activated)
                    clientState.ActiveEditOperationIds.Add(stateChange.EditOperationId);
                else
                    clientState.ActiveEditOperationIds.Remove(stateChange.EditOperationId);
            }
        }

//         void AdvanceState(InterfaceHunkBase stateChange)
//         {
//             
//         }
    }
}