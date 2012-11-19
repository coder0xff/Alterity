using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alterity.Models;


using UserClass = Alterity.Models.User;
using Doredis;

namespace Alterity.Controllers
{
    [Serializable]
    public class NoUserDataEntryException : Exception
    {
        public NoUserDataEntryException() { }
        public NoUserDataEntryException(string message) : base(message) { }
        public NoUserDataEntryException(string message, Exception inner) : base(message, inner) { }
        protected NoUserDataEntryException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    public class AlterityBaseController : Controller
    {
        protected static dynamic SessionState { get { return SessionDataWrapper.GetSessionData(System.Web.HttpContext.Current.Request, System.Web.HttpContext.Current.Response); } }

        protected new UserClass User
        {
            get
            {
                System.Security.Principal.IPrincipal userPrincipal = ((Controller)this).User;
                return UserClass.GetUser(userPrincipal, SessionState, this.Request.UserHostAddress);
            }
        }

        protected void DB(Action action) { EntityMappingContext.Access(action); }

        protected dynamic DS {
            get
            {
                return DataStoreClient.Get();
            }
        }
    }
}
