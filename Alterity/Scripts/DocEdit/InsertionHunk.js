define(["require", "exports", 'IHunk', 'IntegerInterval', 'DeletionHunk'], function(require, exports, __IHunkModule__, __IntegerIntervalModule__, __DeletionHunkModule__) {
    var IHunkModule = __IHunkModule__;

    var IntegerIntervalModule = __IntegerIntervalModule__;

    var DeletionHunkModule = __DeletionHunkModule__;

    (function (Alterity) {
        var InsertionHunk = (function () {
            function InsertionHunk(left, text) {
                this.left = left;
                this.text = text;
                this.type = "insertion";
            }
            Object.defineProperty(InsertionHunk.prototype, "length", {
                get: function () {
                    return this.text.length;
                },
                enumerable: true,
                configurable: true
            });
            Object.defineProperty(InsertionHunk.prototype, "right", {
                get: function () {
                    return this.left + this.length - 1;
                },
                enumerable: true,
                configurable: true
            });
            InsertionHunk.prototype.MergeSubsequent = function (other) {
                if(other.type == "insertion") {
                    var asInsertion = other;
                    if(asInsertion.left >= this.left && asInsertion.left <= this.left + this.length) {
                        var insertionIndex = asInsertion.left - this.left;
                        var newText = this.text.substr(0, insertionIndex) + asInsertion.text + this.text.substr(insertionIndex);
                        this.text = newText;
                        return null;
                    } else {
                        return new InsertionHunk(asInsertion.left, asInsertion.text);
                    }
                } else {
                    if(other.type == "deletion") {
                        var asDeletion = other;
                        if(asDeletion.left + this.length >= this.left && asDeletion.left <= this.left + this.length) {
                            var resultText = "";
                            var remnantIntervals = new IntegerIntervalModule.Alterity.IntegerInterval(this.left, this.text.length).Subtract(new IntegerIntervalModule.Alterity.IntegerInterval(asDeletion.left, asDeletion.length));
                            for(var remnantIndex = 0; remnantIndex < remnantIntervals.length; remnantIndex++) {
                                var remnantInterval = remnantIntervals[remnantIndex];
                                resultText += this.text.substr(remnantInterval.left - this.left, remnantInterval.length);
                            }
                            var resultLength = resultText.length;
                            var mutualAnnihilationLength = this.text.length - resultLength;
                            var deletionRemainderLength = asDeletion.length - mutualAnnihilationLength;
                            this.text = resultText;
                            if(deletionRemainderLength > 0) {
                                return new DeletionHunkModule.Alterity.DeletionHunk(asDeletion.left, deletionRemainderLength);
                            } else {
                                return null;
                            }
                        }
                    }
                }
            };
            return InsertionHunk;
        })();
        Alterity.InsertionHunk = InsertionHunk;        
    })(exports.Alterity || (exports.Alterity = {}));

})

