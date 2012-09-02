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
        public bool ReceiveInsertionHunk(int documentId, int startIndex, String text, int liveHunkStateSequenceIndex)
        {
            Document.GetById(documentId).AppendHunk(new InsertionHunk(startIndex, text), this.User);
            return true;
        }

        [JSRPCNet.ApiMethod]
        public bool ReceiveDeletionHunk(int documentId, int startIndex, int length, int liveHunkStateSequenceIndex)
        {
            EntityMappingContext.Access(() =>
            {
                Document.GetById(documentId).AppendHunk(new DeletionHunk(startIndex, length), this.User);
            });
            return true;
        }
    }
}
