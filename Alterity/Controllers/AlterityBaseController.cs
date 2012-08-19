using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alterity.Models;


using UserClass = Alterity.Models.User;

namespace Alterity.Controllers
{
    public class AlterityBaseController : Controller
    {
        protected dynamic SessionState { get { return SessionDataWrapper.GetSessionData(Request, Response); } }
        protected new User User
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
                        if (result == null) WebMatrix.WebData.WebSecurity.Logout();
                    }
                    else
                    {
                        result = UserClass.CreateAnonymous(this.Request.UserHostAddress);
                        SessionState.UserName = result.UserName;
                    }
                }
                else
                {
                    result = UserClass.GetUserByUserName(SessionState.UserName);
                    if (result == null)
                    {
                        SessionState.UserName = null;
                        return User;
                    }
                }
                return result;
            }
        }
    }
}
