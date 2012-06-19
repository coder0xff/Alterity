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
    public class UserDataController : AlterityBaseController
    {
        public ViewResult Index()
        {
            return View((new EntityMappingContext()).Users.ToList());
        }

        //
        // GET: /UserData/Details/5

        public ViewResult Details(string id)
        {
            User userdata = (new EntityMappingContext()).Users.Find(id);
            return View(userdata);
        }

        //
        // GET: /UserData/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /UserData/Create

        [HttpPost]
        public ActionResult Create(User userdata)
        {
            if (ModelState.IsValid)
            {
                EntityMappingContext.AccessDataBase(() =>
                    {
                        EntityMappingContext.Current.Users.Add(userdata);
                    });
                return RedirectToAction("Index");  
            }

            return View(userdata);
        }
        
        //
        // GET: /UserData/Edit/5
 
        public ActionResult Edit(string id)
        {
            User userdata = (new EntityMappingContext()).Users.Find(id);
            return View(userdata);
        }

        //
        // POST: /UserData/Edit/5

        [HttpPost]
        public ActionResult Edit(User userdata)
        {
            if (ModelState.IsValid)
            {
                EntityMappingContext.AccessDataBase(() =>
                {
                    EntityMappingContext.Current.Entry(userdata).State = EntityState.Modified;
                });
                return RedirectToAction("Index");
            }
            return View(userdata);
        }

        //
        // GET: /UserData/Delete/5
 
        public ActionResult Delete(string id)
        {
            User userdata = (new EntityMappingContext()).Users.Find(id);
            return View(userdata);
        }

        //
        // POST: /UserData/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            EntityMappingContext.AccessDataBase(() =>
                {
                    User userdata = EntityMappingContext.Current.Users.Find(id);
                    EntityMappingContext.Current.Users.Remove(userdata);
                });
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}