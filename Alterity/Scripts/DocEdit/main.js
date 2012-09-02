require("/Scripts/DocEdit/DeletionHunk.js");
require("/Scripts/DocEdit/InsertionHunk.js");

var docEditApi = new JSRPCNet("/api/DocEdit");
var hunkTransmitQueue = new Array();
var lastReceivedUpdateIndex = -1;
var lastTransmittedUpdateIndex = -1;

function BlockDeleted(index, length) {
    var deletionHunk = new DeletionHunk(index, length);
    if (hunkTransmitQueue.length > 0) {
        deletionHunk = hunkTransmitQueue[hunkTransmitQueue.length - 1].mergeSubsequent(deletionHunk)
    }
    if (deletionHunk != null) {
        hunkTransmitQueue.push(deletionHunk);
    }
}

var transmitDataTimer = setInterval(function () {
    if (hunkTransmitQueue.length > 0) {
        docEditApi.ReceiveHunks(documentId, hunkTransmitQueue);
    }
    hunkTransmitQueue.length = 0;
}, 60000)

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