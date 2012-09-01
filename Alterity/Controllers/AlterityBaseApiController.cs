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
        protected dynamic SessionState
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
                User result = null;
                if (SessionState.UserName == null)
                {
                    if (((ApiController)this).User.Identity.IsAuthenticated)
                    {
                        SessionState.UserName = ((ApiController)this).User.Identity.Name;
                        result = User.GetUserByUserName(SessionState.UserName);
                        if (result == null) WebMatrix.WebData.WebSecurity.Logout();
                    }
                    else
                    {
                        result = UserClass.CreateAnonymous(HttpContext.Current.Request.UserHostAddress);
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
