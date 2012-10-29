define(["require", "exports", "InsertionHunk", "DeletionHunk", "NoOpHunk"], function(require, exports, __InsertionHunkModule__, __DeletionHunkModule__, __NoOpHunkModule__) {
    
    var InsertionHunkModule = __InsertionHunkModule__;

    var InsertionHunk = InsertionHunkModule.Alterity.InsertionHunk;
    var DeletionHunkModule = __DeletionHunkModule__;

    var DeletionHunk = DeletionHunkModule.Alterity.DeletionHunk;
    var NoOpHunkModule = __NoOpHunkModule__;

    var docEditApi = new JSRPCNet("/api/DocEdit");
    var documentId = 0;
    var sharedState;
    var hunkTransmitQueue = [];
    var unconfirmedTransmissions = [];
    var outOfOrderReceivedTransmisionConfirmations = [];
    var lastReceivedServerTick = -1;
    var lastTransmittedUpdateIndex = -1;
    function flushTransmitQueue() {
        if(hunkTransmitQueue.length > 0) {
            console.log("transmitting hunks: " + JSON.stringify(hunkTransmitQueue));
            lastTransmittedUpdateIndex++;
            docEditApi.ReceiveHunks(documentId, lastTransmittedUpdateIndex, hunkTransmitQueue);
            unconfirmedTransmissions.push({
                updateIndex: lastTransmittedUpdateIndex,
                hunks: hunkTransmitQueue.slice(0)
            });
            hunkTransmitQueue.length = 0;
        }
    }
    function processReceivedHunk(hunk) {
        for(var unconfirmedTransmissionIndex = 0; unconfirmedTransmissionIndex < unconfirmedTransmissions.length; unconfirmedTransmissionIndex++) {
            var unconfirmedTransmission = unconfirmedTransmissions[unconfirmedTransmissionIndex];
            var transformedHunks = [];
            for(var unconfirmedHunkIndex = 0; unconfirmedHunkIndex < unconfirmedTransmission.hunks.length; unconfirmedHunkIndex++) {
                var untransformedHunk = unconfirmedTransmission.hunks[unconfirmedHunkIndex];
                transformedHunks.concat(untransformedHunk.RedoPrior(untransformedHunk.tick, hunk));
            }
            unconfirmedTransmission.hunks = transformedHunks;
        }
        var transformedHunks = [];
        for(var queuedHunkIndex = 0; queuedHunkIndex < hunkTransmitQueue.length; queuedHunkIndex++) {
            var untransformedHunk = hunkTransmitQueue[queuedHunkIndex];
            transformedHunks.concat(untransformedHunk.RedoPrior(untransformedHunk.tick, hunk));
        }
        hunkTransmitQueue = transformedHunks;
        sharedState = hunk.Apply(sharedState);
    }
    function processReceivedHunks(serverTick, hunks) {
        var selectionRange = getSelectionRange();
        var selectionHunk = new NoOpHunkModule.Alterity.NoOpHunk(0, selectionRange.start, selectionRange.start - selectionRange.end);
        for(var hunkIndex = 0; hunkIndex < hunks.length; hunkIndex++) {
            var hunk = hunks[hunkIndex];
            selectionRange.processReceivedHunk(hunk);
        }
        for(var queuedHunkIndex = 0; queuedHunkIndex < hunkTransmitQueue.length; queuedHunkIndex++) {
            var untransformedHunk = hunkTransmitQueue[queuedHunkIndex];
            assert(untransformedHunk.tick == serverTick - 1, "Cannot advance a hunk more than one tick at a time. Something was missed!");
            untransformedHunk.tick = serverTick;
        }
        updatePredictedState();
    }
    function redoTransformNoOpHunks(noOps, hunk) {
        var result = [];
        for(var noOpIndex = 0; noOpIndex < noOps.length; noOpIndex++) {
            result.concat(noOps[noOpIndex].RedoPrior(0, hunk));
        }
        return result;
    }
    function undoTransformNoOpHunks(noOps, hunk) {
        var result = [];
        for(var noOpIndex = 0; noOpIndex < noOps.length; noOpIndex++) {
            result.concat(noOps[noOpIndex].UndoPrior(0, hunk));
        }
        return result;
    }
    function updatePredictedState() {
        var predictedState = sharedState;
        for(var unconfirmedTransmissionIndex = 0; unconfirmedTransmissionIndex < unconfirmedTransmissions.length; unconfirmedTransmissionIndex++) {
            var unconfirmedTransmission = unconfirmedTransmissions[unconfirmedTransmissionIndex];
            for(var unconfirmedHunkIndex = 0; unconfirmedHunkIndex < unconfirmedTransmission.hunks.length; unconfirmedHunkIndex++) {
                var hunk = unconfirmedTransmission.hunks[unconfirmedHunkIndex];
                predictedState = hunk.Apply(predictedState);
            }
        }
        for(var queuedHunkIndex = 0; queuedHunkIndex < hunkTransmitQueue.length; queuedHunkIndex++) {
            var queuedHunk = hunkTransmitQueue[queuedHunkIndex];
            predictedState = queuedHunk.Apply(predictedState);
        }
        $("#plainTextArea").val(predictedState);
    }
    function processTransmissionConfirmation(updateIndex, tick) {
        assert(unconfirmedTransmissions[0].updateIndex == updateIndex, "transmission confirmations must be processed in order.");
        var transmission = unconfirmedTransmissions.shift();
        for(var hunkIndex = 0; hunkIndex < transmission.hunks.length; hunkIndex++) {
            var confirmedHunk = transmission.hunks[hunkIndex];
            sharedState = confirmedHunk.Apply(sharedState);
        }
        for(var queuedHunkIndex = 0; queuedHunkIndex < hunkTransmitQueue.length; queuedHunkIndex++) {
            assert(hunkTransmitQueue[queuedHunkIndex].tick == tick - 1, "Cannot advance a hunk more than one tick at a time. Something was missed!");
            hunkTransmitQueue[queuedHunkIndex].tick = tick;
        }
    }
    var transmitDataTimer = setInterval(function () {
        flushTransmitQueue();
    }, 10000);
    function getSelectionRange() {
        return ($("#plainTextArea")).selection();
    }
    function setSelectionRange(start, end) {
        ($("plainTextArea")).selection(start, end);
    }
    function blockDeleted(index, length) {
        console.log("deleted at: " + index + " " + length);
        var deletionHunk = new DeletionHunk(lastReceivedServerTick, index, length);
        if(hunkTransmitQueue.length > 0) {
            deletionHunk = hunkTransmitQueue[hunkTransmitQueue.length - 1].MergeSubsequent(deletionHunk);
        }
        if(deletionHunk != null) {
            hunkTransmitQueue.push(deletionHunk);
        }
    }
    function blockInserted(index, text) {
        console.log("inserted at: " + index + " " + text);
        var insertionHunk = new InsertionHunk(lastReceivedServerTick, index, text);
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

