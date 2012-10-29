using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alterity.Models;


using UserClass = Alterity.Models.User;

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
                UserClass result = null;
                if (SessionState.UserName == null)
                {
                    if (((Controller)this).User.Identity.IsAuthenticated)
                    {
                        string UserName = ((Controller)this).User.Identity.Name;
                        SessionState.UserName = UserName;
                        result = User.GetUserByUserName(UserName);
                        if (result == null)
                        {
                            //SimpleMembershipProvider thinks we're logged in, but the account doesn't exist!
                            //Logout and return a new anonymous user
                            AccountController.InternalLogout();
                            result = UserClass.CreateAnonymous(this.Request.UserHostAddress);
                            SessionState.UserName = result.UserName;
                        }
                    }
                    else
                    {
                        result = UserClass.CreateAnonymous(this.Request.UserHostAddress);
                        SessionState.UserName = result.UserName;
                    }
                }
                else
                {
                    string userName = SessionState.UserName;
                    System.Diagnostics.Debug.Assert(!((Controller)this).User.Identity.IsAuthenticated || ((Controller)this).User.Identity.Name == userName);
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
        protected void DB(Action action) { EntityMappingContext.Access(action); }
    }
}
