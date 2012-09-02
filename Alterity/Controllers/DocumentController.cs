using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Alterity.Controllers
{
    public class DocumentController : Controller
    {
        public ActionResult Edit()
        {
            ViewBag.DocumentId = 0;
            return View();
        }
    }
}
