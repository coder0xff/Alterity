using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alterity.Models;

namespace Alterity.Controllers
{ 
    public class UserController : AlterityBaseController
    {
        public ViewResult Index()
        {
            EntityMappingContext.Access(() =>
                {
                    User.Documents.Remove(User.Documents.First());
                });
            return View((new EntityMappingContext()).Users.ToList());
        }

        //
        // GET: /User/Details/5

        public ViewResult Details(string id)
        {
            User user = (new EntityMappingContext()).Users.Find(id);
            return View(user);
        }

        //
        // GET: /User/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /User/Create

        [HttpPost]
        public ActionResult Create(User user)
        {
            if (ModelState.IsValid)
            {
                EntityMappingContext.Access(() =>
                    {
                        EntityMappingContext.Current.Users.Add(user);
                    });
                return RedirectToAction("Index");  
            }

            return View(user);
        }
        
        //
        // GET: /User/Edit/5
 
        public ActionResult Edit(string id)
        {
            User user = (new EntityMappingContext()).Users.Find(id);
            return View(user);
        }

        //
        // POST: /User/Edit/5

        [HttpPost]
        public ActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                EntityMappingContext.Access(() =>
                {
                    EntityMappingContext.Current.Entry(user).State = EntityState.Modified;
                });
                return RedirectToAction("Index");
            }
            return View(user);
        }

        //
        // GET: /User/Delete/5
 
        public ActionResult Delete(string id)
        {
            User user = (new EntityMappingContext()).Users.Find(id);
            return View(user);
        }

        //
        // POST: /User/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            EntityMappingContext.Access(() =>
                {
                    User user = EntityMappingContext.Current.Users.Find(id);
                    EntityMappingContext.Current.Users.Remove(user);
                });
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}