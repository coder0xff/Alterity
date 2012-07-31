using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alterity.Models;
using Alterity.Models.Async;

namespace Alterity.Controllers
{
    public class AsyncController : AlterityBaseController
    {
        //
        // GET: /Async/

        public ActionResult Index()
        {
            return RedirectPermanent("/");
        }

        [HttpPost]
        public ActionResult ReceiveLiveHunk(LiveHunk liveHunk)
        {
            EntityMappingContext.Access(() =>
            {
                Hunk hunk = liveHunk.Convert();
                Document document = Document.GetById(liveHunk.documentId);

            });
            return new EmptyResult();
        }
    }
}
