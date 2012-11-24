using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models.Async
{
    public class DocumentEditState
    {
        dynamic dataObject;

        public static implicit operator DocumentEditState(System.Dynamic.DynamicObject dataObject)
        {
            DocumentEditState result = new DocumentEditState();
            result.dataObject = dataObject;
            return result;
        }

        public ClientStateCollection ClientStates
        {
            get
            {
                return dataObject.ClientStates;
            }
        }

        
    }
}