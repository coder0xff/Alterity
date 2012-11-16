using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alterity.Models.Async
{
    public class ClientStateCollection
    {
        dynamic dataObject;

        public static implicit operator ClientStateCollection(Doredis.DynamicDataObject dataObject)
        {
            ClientStateCollection result = new ClientStateCollection();
            result.dataObject = dataObject;
            return result;
        }

        public ClientState this[User user]
        {
            get
            {
                return dataObject[user.Id.ToString()];
            }
        }
    }
}