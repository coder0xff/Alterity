using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Alterity.Models;
using Alterity.Models.Async;

namespace Alterity.Controllers
{
    public class DocEditController : AlterityBaseApiController
    {
        [JSRPCNet.ApiMethod]
        public void ReceiveHunks(int documentId, HunkDTO[] hunkDTOs)
        {
            List<Hunk> hunks = new List<Hunk>(hunkDTOs.Select(_ => _.Convert()));
            return;
        }
    }
}
