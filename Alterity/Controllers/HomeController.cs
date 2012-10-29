using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Alterity.Controllers
{
    public class HomeController : AlterityBaseController
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Facilitating the collective voice of The Internet.";
            DB(() =>
            {
                ViewBag.IsAnonymousUser = User.IsAnonymous;

                if (!ViewBag.IsAnonymousUser)
                    ViewBag.UserDocuments = new List<Alterity.Models.Document>(User.Documents);
            });
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Alterity is a peer-moderated, concurrent document editor.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
