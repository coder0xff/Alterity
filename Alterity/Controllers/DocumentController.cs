using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alterity.Models;

namespace Alterity.Controllers
{
    public class DocumentController : AlterityBaseController
    {
        public ActionResult Edit(int id)
        {
            ViewBag.DocumentId = id;
            return View();
        }

        //
        // GET: /DocumentTemp/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /DocumentTemp/Create

        [HttpPost]
        public ActionResult Create(Document document)
        {
            if (ModelState.IsValid)
            {
                DB(() => {
                    document.Owner = User;
                    document = EntityMappingContext.Current.Documents.Add(document);
                });
                return RedirectToAction("Edit", new { id = document.Id });
            }

            return View(document);
        }
    }
}
