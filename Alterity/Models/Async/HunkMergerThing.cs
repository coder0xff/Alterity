using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Alterity.Models;

namespace Alterity.Models.Async
{
    public class HunkMergerThing
    {
        internal void ReceiveHunks(Document document, User user, int clientUpdateStamp, int serverUpdateStamp, Hunk[] hunks)
        {
            DocumentEditStateCollection documentEditStates = DataStoreClient.Get();
            DocumentEditState documentEditState = documentEditStates[document];

        }
    }
}