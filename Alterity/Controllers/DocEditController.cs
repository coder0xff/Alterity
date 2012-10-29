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
        public void ReceiveHunks(int documentId, int clientTransmittedUpdateIndex, HunkDTO[] hunkDTOs)
        {
            List<Hunk> hunks = new List<Hunk>(hunkDTOs.Select(_ => _.Convert()));
            foreach (Hunk hunk in hunks)
                System.Diagnostics.Debug.WriteLine(hunk.GetType().ToString() + " " + hunk.ToString());
            return;
        }

        [JSRPCNet.ApiMethod]
        public String Test(int anInt, String aString)
        {
            return "Hello world! " + anInt.ToString(System.Globalization.CultureInfo.CurrentCulture) + " " + aString;
        }
    }
}
