using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alterity.Models;

namespace Alterity.Controllers
{
    public class AlterityController : Controller
    {
        protected EntityMappingContext db = new EntityMappingContext();
        public EntityMappingContext GetDB() { return db; }
        protected dynamic SessionState { get { return SessionDataWrapper.GetSessionData(db, Request, Response); } }
        protected void Save() { db.SaveChanges(); }
    }
}
