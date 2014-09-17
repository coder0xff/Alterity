using Alterity.Models;
using Alterity.Models.Async;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Alterity.Controllers
{
    public class DocEditController : AlterityBaseApiController
    {
        [JSRPCNet.ApiMethod]
        public void ReceiveHunks(int documentId, int serverUpdateStamp, int clientUpdateStamp, IEnumerable<HunkDTO> hunkDtos)
        {
            DB(() =>
            {
                var document = EntityMappingContext.Current.Documents.First(x => x.Id == documentId);
                DocumentEditState documentEditState = DS.DocumentEditStates[document];
                PerDocumentClientState clientState = documentEditState.ClientStates[User];
                if (clientUpdateStamp == clientState.ClientUpdateIndex + 1)
                {
                    // got the index we expected!
                    // apply this update
                    // and then get stored updates one at a time from the sorted store
                    foreach (var hunk in hunkDtos.Select(hunkDto => hunkDto.Convert()))
                    {
                        document.AppendHunk(hunk, User);
                    }
                }
                else
                {
                    //store it for subsequent application
                }
            });
            var hunks = new List<Hunk>(hunkDtos.Select(_ => _.Convert()));
            foreach (Hunk hunk in hunks)
                System.Diagnostics.Debug.WriteLine(hunk.GetType() + " " + hunk);
        }


        [JSRPCNet.ApiMethod]
        public String Test(int anInt, String aString)
        {
            return "Hello world! " + anInt.ToString(System.Globalization.CultureInfo.CurrentCulture) + " " + aString;
        }
    }
}
