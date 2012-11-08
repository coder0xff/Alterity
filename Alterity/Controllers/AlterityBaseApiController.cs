using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Alterity.Models;

using UserClass = Alterity.Models.User;

namespace Alterity.Controllers
{
    public class AlterityBaseApiController : JSRPCNet.ApiController
    {
        static protected dynamic SessionState
        {
            get
            {
                return SessionDataWrapper.GetSessionData(HttpContext.Current.Request, HttpContext.Current.Response);
            }
        }

        protected new User User
        {
            get
            {
                System.Security.Principal.IPrincipal userPrincipal = ((ApiController)this).User;
                return UserClass.GetUser(userPrincipal, SessionState, HttpContext.Current.Request.UserHostAddress);
            }
        }

        protected void DB(Action action) { EntityMappingContext.Access(action); }
    }
}
