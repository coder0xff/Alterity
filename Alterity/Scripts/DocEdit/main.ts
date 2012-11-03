/// <reference path='../../JSRPCNet/JSRPCNet.d.ts'/>
/// <reference path="../jquery.d.ts" />
/// <reference path="../assert.ts" />

import IHunkModule = module("IHunk");
import InsertionHunkModule = module("InsertionHunk");
var InsertionHunk = InsertionHunkModule.Alterity.InsertionHunk;
import DeletionHunkModule = module("DeletionHunk");
var DeletionHunk = DeletionHunkModule.Alterity.DeletionHunk;
import NoOpHunkModule = module("NoOpHunk");

var docEditApi = new JSRPCNet("/api/DocEdit");
var documentId = 0;

//the most recent document state corroborated by the server
var sharedState: string;
//hunks are constructed, manipulated, and ultimately transmitted from this array
var hunkTransmitQueue: IHunkModule.Alterity.IHunk[] = [];

interface UnconfirmedTransmitInfo {
    updateIndex: number;
    hunks: IHunkModule.Alterity.IHunk[];
}
var unconfirmedTransmissions = [];

//While TCP transactions are ordered, separate TCP transactions are not
//so we store confirmations and incoming hunks here if they arrive out of order
var outOfOrderReceivedTransmisionConfirmations = [];
var serverUpdateStamp = -1;
var clientUpdateStamp = -1;

    function flushTransmitQueue() {
        if (hunkTransmitQueue.length > 0) {
            console.log("transmitting hunks: " + JSON.stringify(hunkTransmitQueue));
            clientUpdateStamp++;
            docEditApi.ReceiveHunks(documentId, clientUpdateStamp, hunkTransmitQueue);
            unconfirmedTransmissions.push({ updateIndex: clientUpdateStamp, hunks: hunkTransmitQueue.slice(0) });
            hunkTransmitQueue.length = 0;
        }
    }

    // Apply a hunk to UnconfirmedTransmitInfo(s) in unconfirmedTransmission (but don't advance their indices)
    // apply it to queued hunks (but don't advance their indices)
    // apply it to the shared state
    function processReceivedHunk(hunk: IHunkModule.Alterity.IHunk) {
        for (var unconfirmedTransmissionIndex = 0; unconfirmedTransmissionIndex < unconfirmedTransmissions.length; unconfirmedTransmissionIndex++) {
            var unconfirmedTransmission = unconfirmedTransmissions[unconfirmedTransmissionIndex];
            var transformedHunks: IHunkModule.Alterity.IHunk[] = [];
            for (var unconfirmedHunkIndex = 0; unconfirmedHunkIndex < unconfirmedTransmission.hunks.length; unconfirmedHunkIndex++) {
                var untransformedHunk = unconfirmedTransmission.hunks[unconfirmedHunkIndex];
                transformedHunks.concat(untransformedHunk.RedoPrior(untransformedHunk.tick, hunk));
            }
            unconfirmedTransmission.hunks = transformedHunks;
        }
        var transformedHunks: IHunkModule.Alterity.IHunk[] = [];
        for (var queuedHunkIndex = 0; queuedHunkIndex < hunkTransmitQueue.length; queuedHunkIndex++) {
            var untransformedHunk = hunkTransmitQueue[queuedHunkIndex];
            transformedHunks.concat(untransformedHunk.RedoPrior(untransformedHunk.tick, hunk));
        }
        hunkTransmitQueue = transformedHunks;
        sharedState = hunk.Apply(sharedState);
    }

    // Use processReceivedHunk for each hunk
    // Update queued hunks' tick
    // call updatePredictedState
    // set selection range
    function processReceivedHunks(serverTick: number, hunks: IHunkModule.Alterity.IHunk[]) {
        var selectionRange = getSelectionRange(); //must do this before updatePredictedState since it will destroy the selection
        var selectionHunk = new NoOpHunkModule.Alterity.NoOpHunk(0, selectionRange.start, selectionRange.start - selectionRange.end);
        for (var hunkIndex = 0; hunkIndex < hunks.length; hunkIndex++) {
            var hunk = hunks[hunkIndex];
            selectionRange.
            processReceivedHunk(hunk);
        }
        for (var queuedHunkIndex = 0; queuedHunkIndex < hunkTransmitQueue.length; queuedHunkIndex++) {
            var untransformedHunk = hunkTransmitQueue[queuedHunkIndex];
            assert(untransformedHunk.tick == serverTick - 1, "Cannot advance a hunk more than one tick at a time. Something was missed!");
            untransformedHunk.tick = serverTick;
        }
        updatePredictedState();

    }    

    function redoTransformNoOpHunks(noOps: NoOpHunkModule.Alterity.NoOpHunk[], hunk: IHunkModule.Alterity.IHunk) {
        var result = [];
        for (var noOpIndex = 0; noOpIndex < noOps.length; noOpIndex++)
            result.concat(noOps[noOpIndex].RedoPrior(0, hunk));
        return result;
    }

    function undoTransformNoOpHunks(noOps: NoOpHunkModule.Alterity.NoOpHunk[], hunk: IHunkModule.Alterity.IHunk) {
        var result = [];
        for (var noOpIndex = 0; noOpIndex < noOps.length; noOpIndex++)
            result.concat(noOps[noOpIndex].UndoPrior(0, hunk));
        return result;
    }

    function updatePredictedState() {
        var predictedState = sharedState;
        for (var unconfirmedTransmissionIndex = 0; unconfirmedTransmissionIndex < unconfirmedTransmissions.length; unconfirmedTransmissionIndex++) {
            var unconfirmedTransmission = unconfirmedTransmissions[unconfirmedTransmissionIndex];
            for (var unconfirmedHunkIndex = 0; unconfirmedHunkIndex < unconfirmedTransmission.hunks.length; unconfirmedHunkIndex++) {
                var hunk = unconfirmedTransmission.hunks[unconfirmedHunkIndex];
                predictedState = hunk.Apply(predictedState);
            }
        }
        for (var queuedHunkIndex = 0; queuedHunkIndex < hunkTransmitQueue.length; queuedHunkIndex++) {
            var queuedHunk = hunkTransmitQueue[queuedHunkIndex];
            predictedState = queuedHunk.Apply(predictedState);
        }
        $("#plainTextArea").val(predictedState);
    }

    // Remove the corresponding UnconfirmedTransmitInfo
    // apply the hunks to the sharedState
    // and advance the server tick on all elements of hunkTransmissionQueue
    function processTransmissionConfirmation(updateIndex: number, tick: number) {
        assert(unconfirmedTransmissions[0].updateIndex == updateIndex, "transmission confirmations must be processed in order.");
        var transmission = unconfirmedTransmissions.shift();
        for (var hunkIndex = 0; hunkIndex < transmission.hunks.length; hunkIndex++) {
            var confirmedHunk = transmission.hunks[hunkIndex];
            sharedState = confirmedHunk.Apply(sharedState);
        }
        for (var queuedHunkIndex = 0; queuedHunkIndex < hunkTransmitQueue.length; queuedHunkIndex++) {
            assert(hunkTransmitQueue[queuedHunkIndex].tick == tick - 1, "Cannot advance a hunk more than one tick at a time. Something was missed!");
            hunkTransmitQueue[queuedHunkIndex].tick = tick;
        }
    }

    var transmitDataTimer = setInterval(function () {
        flushTransmitQueue();
    }, 10000)

    function getSelectionRange() {
        return (<any>$("#plainTextArea")).selection();
    }

    function setSelectionRange(start: number, end: number) {
        (<any>$("plainTextArea")).selection(start, end);
    }

    function blockDeleted(index, length) {
        console.log("deleted at: " + index + " " + length);
        var deletionHunk = new DeletionHunk(serverUpdateStamp, index, length);
        if (hunkTransmitQueue.length > 0) {
            deletionHunk = <DeletionHunkModule.Alterity.DeletionHunk>hunkTransmitQueue[hunkTransmitQueue.length - 1].MergeSubsequent(deletionHunk)
        }
        if (deletionHunk != null) {
            hunkTransmitQueue.push(deletionHunk);
        }
    }

    function blockInserted(index, text) {
        console.log("inserted at: " + index + " " + text);
        var insertionHunk = new InsertionHunk(serverUpdateStamp, index, text);
        if (hunkTransmitQueue.length > 0) {
            insertionHunk = <InsertionHunkModule.Alterity.InsertionHunk>hunkTransmitQueue[hunkTransmitQueue.length - 1].MergeSubsequent(insertionHunk);
        }
        if (insertionHunk != null) {
            hunkTransmitQueue.push(insertionHunk);
        }
    }

    function deleteBlock(startIndex, length) {
        if (length == 0) return;
        var textArea = $("#plainTextArea");
        var previousText = textArea.val()
        textArea.val(previousText.substr(0, startIndex) + previousText.substr(startIndex + length));
        blockDeleted(startIndex, length);
    }

    function insertBlock(startIndex, text) {
        if (text.length == 0) return;
        var textArea = $("#plainTextArea");
        var previousText = textArea.val()
        textArea.val(previousText.substr(0, startIndex) + text + previousText.substr(startIndex));
        blockInserted(startIndex, text);
    }

    $(function () {
        //var editor = CKEDITOR.replace("plainTextArea");
        var textArea = $("#plainTextArea");
        textArea.on("keypress", function (event) {
            var selectionRange = (<any>textArea).selection();
            if (selectionRange.end - selectionRange.start > 0) {
                blockDeleted(selectionRange.start, selectionRange.end - selectionRange.start);
            }
            blockInserted(selectionRange.start, String.fromCharCode(event.which));
        });

        textArea.on("keydown", function (event) {
            var selectionRange = (<any>textArea).selection();

            //backspace key
            if (event.which == 8) {
                event.preventDefault();
                if (selectionRange.start != selectionRange.end) {
                    deleteBlock(selectionRange.start, selectionRange.end - selectionRange.start);
                    (<any>textArea).selection(selectionRange.start, selectionRange.start);
                }
                else if (selectionRange.start > 0) {
                    deleteBlock(selectionRange.start - 1, 1);
                    (<any>textArea).selection(selectionRange.start - 1, selectionRange.start - 1);
                }
            }
            //tab key
            if (event.which == 9) {
                event.preventDefault();
                insertBlock(selectionRange.start, String.fromCharCode(9));
                (<any>textArea).selection(selectionRange.start + 1, selectionRange.start + 1);
            }
            //delete key
            if (event.which == 46) {
                event.preventDefault();
                if (selectionRange.start != selectionRange.end) {
                    deleteBlock(selectionRange.start, selectionRange.end - selectionRange.start);
                }
                else if (selectionRange.end < textArea.val().length) {
                    deleteBlock(selectionRange.start, 1);
                }
                (<any>textArea).selection(selectionRange.start, selectionRange.start);
            }
        });
    });