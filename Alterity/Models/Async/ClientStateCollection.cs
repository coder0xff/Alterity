using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models.Async
{
    public class ClientStateCollection
    {
        dynamic dataObject;

        public static implicit operator ClientStateCollection(System.Dynamic.DynamicObject dataObject)
        {
            ClientStateCollection result = new ClientStateCollection();
            result.dataObject = dataObject;
            return result;
        }

        public PerDocumentClientState this[User user]
        {
            get
            {
                return dataObject[user.Id.ToString()];
            }
        }
    }
}