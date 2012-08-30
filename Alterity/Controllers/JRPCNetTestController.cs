using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Alterity.Controllers
{
    public class JRPCNetTestController : JRPCNet.Api
    {
        [HttpPost]
        public bool Test(Alterity.Models.Async.HunkDTO hunk, String primitive1, int primitive2)
        {
            return true;
        }
    }
}
