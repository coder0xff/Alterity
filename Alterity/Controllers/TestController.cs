using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Alterity.Controllers
{
    public class TestController : ApiController
    {
        [HttpPost]
        public bool ReceiveHunk(Alterity.Models.Async.HunkDTO hunk)
        {
            return true;
        }

        [HttpGet]
        [HttpPost]
        public bool A(Alterity.Models.Async.HunkDTO hunk)
        {
            return false;
        }
        [HttpGet]
        [HttpPost]
        public bool B() { return true; }
    }
}
