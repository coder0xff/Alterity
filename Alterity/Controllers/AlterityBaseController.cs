using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alterity.Models;

namespace Alterity.Controllers
{
    public class AlterityBaseController : Controller
    {
        protected dynamic SessionState { get { return SessionDataWrapper.GetSessionData(Request, Response); } }
        protected User User
        {
            get
            {
                User result = null;
                if (SessionState.UserName == null)
                {
                    if (((Controller)this).User.Identity.IsAuthenticated)
                    {
                        SessionState.UserName = ((Controller)this).User.Identity.Name;
                        result = User.GetUserByUserName(SessionState.UserName);
                        if (result == null) throw new ApplicationException("User is authenticated, but has no data.");
                    }
                    else
                    {
                        result = User.CreateAnonymous(this.Request.UserHostAddress);
                    }
                }
                else
                {
                    result = User.GetUserByUserName(SessionState.UserName);
                    if (result == null) throw new ApplicationException("User has a session, but has no data.");
                }
                throw new NotImplementedException();
            }
        }
    }
}
