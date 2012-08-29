using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Alterity.Controllers
{
    public class SlimApiTestController : SlimAPI.SlimApi
    {
        [HttpPost]
        public bool Test(Alterity.Models.Async.HunkDTO hunk)
        {
            return true;
        }
    }
}
