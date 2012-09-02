requirejs(["DeletionHunk", "InsertionHunk"], function () {
    var docEditApi = new JSRPCNet("/api/DocEdit");
    var hunkTransmitQueue = new Array();

    function BlockDeleted(index, length) {
        DocEditApi.ReceiveDeletionHunk(@ViewBag.DocumentId , index, length, 0); 
    }
    function CharacterInserted(index, character) {
    }
    $(function () {
        $("#plainTextArea").keypress(function(event) {
            var selectionRange = $("#plainTextArea").selection();
            if (selectionRange.end - selectionRange.start > 0) {
                BlockDeleted(selectionRange.start, selectionRange.end - selectionRange.start);
            }
        });
    });
});