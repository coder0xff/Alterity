using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Doredis;

namespace Alterity.Models.Async
{
    public class ClientState
    {
        dynamic dataObject;

        public static implicit operator ClientState(Doredis.DynamicDataObject dataObject)
        {
            ClientState result = new ClientState();
            result.dataObject = dataObject;
            return result;
        }

        public int ClientUpdateIndex
        {
            get
            {
                return dataObject.clientUpdateIndex;
            }
            set
            {
                dataObject.clientUpdateIndex = value;
            }
        }

        public int ServerUpdateIndex
        {
            get
            {
                return dataObject.ServerUpdateIndex;
            }
            set
            {
                dataObject.ServerUpdateIndex = value;
            }
        }
    }
}