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
    public class UserAccountController : Controller
    {
        private EntityMappingContext db = new EntityMappingContext();

        //
        // GET: /UserAccount/

        public ViewResult Index()
        {
            return View(db.Users.ToList());
        }

        //
        // GET: /UserAccount/Details/5

        public ViewResult Details(int id)
        {
            UserAccount useraccount = db.Users.Find(id);
            return View(useraccount);
        }

        //
        // GET: /UserAccount/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /UserAccount/Create

        [HttpPost]
        public ActionResult Create(UserAccount userAccount)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(userAccount);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(userAccount);
        }
        
        //
        // GET: /UserAccount/Edit/5
 
        public ActionResult Edit(int id)
        {
            UserAccount useraccount = db.Users.Find(id);
            return View(useraccount);
        }

        //
        // POST: /UserAccount/Edit/5

        [HttpPost]
        public ActionResult Edit(UserAccount userAccount)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userAccount).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(userAccount);
        }

        //
        // GET: /UserAccount/Delete/5
 
        public ActionResult Delete(int id)
        {
            UserAccount useraccount = db.Users.Find(id);
            return View(useraccount);
        }

        //
        // POST: /UserAccount/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            UserAccount useraccount = db.Users.Find(id);
            db.Users.Remove(useraccount);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}