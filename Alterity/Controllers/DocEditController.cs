using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Alterity.Models;

namespace Alterity.Controllers
{
    public class DocEditController : AlterityBaseApiController
    {
        [JSRPCNet.ApiMethod]
        bool ReceiveInsertionHunk(int documentId, int startIndex, String text, int liveHunkStateIndex)
        {
            Document.GetById(documentId).AppendHunk(new InsertionHunk(startIndex, text), this.User);
            return true;
        }
    }
}
