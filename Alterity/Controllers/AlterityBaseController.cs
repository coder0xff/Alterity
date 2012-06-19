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
    }
}
