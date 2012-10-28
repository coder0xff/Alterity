define(["require", "exports", "InsertionHunk", "DeletionHunk"], function(require, exports, __InsertionHunkModule__, __DeletionHunkModule__) {
    var InsertionHunkModule = __InsertionHunkModule__;

    var InsertionHunk = InsertionHunkModule.Alterity.InsertionHunk;
    var DeletionHunkModule = __DeletionHunkModule__;

    var DeletionHunk = DeletionHunkModule.Alterity.DeletionHunk;
    var docEditApi = new JSRPCNet("/api/DocEdit");
    var documentId = 0;
    var hunkTransmitQueue = new Array();
    var lastReceivedUpdateIndex = -1;
    var lastTransmittedUpdateIndex = -1;
    function flushTransmitQueue() {
        if(hunkTransmitQueue.length > 0) {
            console.log("transmitting hunks: " + JSON.stringify(hunkTransmitQueue));
            docEditApi.ReceiveHunks(documentId, hunkTransmitQueue);
        }
        hunkTransmitQueue.length = 0;
    }
    function processReceivedHunks(hunks) {
        var textArea = $("#plainTextArea");
        var hunkCount = hunks.length;
        for(var hunkIndex = 0; hunkIndex < hunkCount; hunkIndex++) {
            var hunk = hunks[hunkIndex];
            if(hunk.text === undefined) {
            }
        }
    }
    var transmitDataTimer = setInterval(function () {
        flushTransmitQueue();
    }, 10000);
    function blockDeleted(index, length) {
        console.log("deleted at: " + index + " " + length);
        var deletionHunk = new DeletionHunk(index, length);
        if(hunkTransmitQueue.length > 0) {
            deletionHunk = hunkTransmitQueue[hunkTransmitQueue.length - 1].MergeSubsequent(deletionHunk);
        }
        if(deletionHunk != null) {
            hunkTransmitQueue.push(deletionHunk);
        }
    }
    function blockInserted(index, text) {
        console.log("inserted at: " + index + " " + text);
        var insertionHunk = new InsertionHunk(index, text);
        if(hunkTransmitQueue.length > 0) {
            insertionHunk = hunkTransmitQueue[hunkTransmitQueue.length - 1].MergeSubsequent(insertionHunk);
        }
        if(insertionHunk != null) {
            hunkTransmitQueue.push(insertionHunk);
        }
    }
    function deleteBlock(startIndex, length) {
        if(length == 0) {
            return;
        }
        var textArea = $("#plainTextArea");
        var previousText = textArea.val();
        textArea.val(previousText.substr(0, startIndex) + previousText.substr(startIndex + length));
        blockDeleted(startIndex, length);
    }
    function insertBlock(startIndex, text) {
        if(text.length == 0) {
            return;
        }
        var textArea = $("#plainTextArea");
        var previousText = textArea.val();
        textArea.val(previousText.substr(0, startIndex) + text + previousText.substr(startIndex));
        blockInserted(startIndex, text);
    }
    $(function () {
        var textArea = $("#plainTextArea");
        textArea.on("keypress", function (event) {
            var selectionRange = (textArea).selection();
            if(selectionRange.end - selectionRange.start > 0) {
                blockDeleted(selectionRange.start, selectionRange.end - selectionRange.start);
            }
            blockInserted(selectionRange.start, String.fromCharCode(event.which));
        });
        textArea.on("keydown", function (event) {
            var selectionRange = (textArea).selection();
            if(event.which == 8) {
                event.preventDefault();
                if(selectionRange.start != selectionRange.end) {
                    deleteBlock(selectionRange.start, selectionRange.end - selectionRange.start);
                    (textArea).selection(selectionRange.start, selectionRange.start);
                } else {
                    if(selectionRange.start > 0) {
                        deleteBlock(selectionRange.start - 1, 1);
                        (textArea).selection(selectionRange.start - 1, selectionRange.start - 1);
                    }
                }
            }
            if(event.which == 9) {
                event.preventDefault();
                insertBlock(selectionRange.start, String.fromCharCode(9));
                (textArea).selection(selectionRange.start + 1, selectionRange.start + 1);
            }
            if(event.which == 46) {
                event.preventDefault();
                if(selectionRange.start != selectionRange.end) {
                    deleteBlock(selectionRange.start, selectionRange.end - selectionRange.start);
                } else {
                    if(selectionRange.end < textArea.val().length) {
                        deleteBlock(selectionRange.start, 1);
                    }
                }
                (textArea).selection(selectionRange.start, selectionRange.start);
            }
        });
    });
})

