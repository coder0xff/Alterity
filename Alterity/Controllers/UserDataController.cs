﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alterity.Models;

namespace Alterity.Controllers
{ 
    public class UserDataController : Controller
    {
        private EntityMappingContext db = new EntityMappingContext();

        //
        // GET: /UserData/

        public ViewResult Index()
        {
            return View(db.UserData.ToList());
        }

        //
        // GET: /UserData/Details/5

        public ViewResult Details(string id)
        {
            UserData userdata = db.UserData.Find(id);
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
        public ActionResult Create(UserData userdata)
        {
            if (ModelState.IsValid)
            {
                db.UserData.Add(userdata);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(userdata);
        }
        
        //
        // GET: /UserData/Edit/5
 
        public ActionResult Edit(string id)
        {
            UserData userdata = db.UserData.Find(id);
            return View(userdata);
        }

        //
        // POST: /UserData/Edit/5

        [HttpPost]
        public ActionResult Edit(UserData userdata)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userdata).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(userdata);
        }

        //
        // GET: /UserData/Delete/5
 
        public ActionResult Delete(string id)
        {
            UserData userdata = db.UserData.Find(id);
            return View(userdata);
        }

        //
        // POST: /UserData/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {            
            UserData userdata = db.UserData.Find(id);
            db.UserData.Remove(userdata);
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