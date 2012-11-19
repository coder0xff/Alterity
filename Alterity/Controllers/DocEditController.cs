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
        public void ReceiveHunks(int documentId, int serverUpdateStamp, int clientUpdateStamp, HunkDTO[] hunkDTOs)
        {
            DB(() =>
            {
                Document document = EntityMappingContext.Current.Documents.First(x => x.Id == documentId);
                DocumentEditStateCollection documentEditStates = DS.DocumentEditStates;
                ClientState clientState = documentEditStates[document].ClientStates[User];
                if (clientUpdateStamp == clientState.ClientUpdateIndex + 1)
                {
                    // got the index we expected!
                    // apply this update
                    // and then get stored updates one at a time from the sorted store
                    // removing them in the process using a transaction

                }
                else
                {
                    //store it for subsequent application
                }
            });
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
