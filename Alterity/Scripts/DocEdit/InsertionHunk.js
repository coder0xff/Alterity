define(["require", "exports", 'IHunk', 'IntegerInterval', 'DeletionHunk'], function(require, exports, __IHunkModule__, __IntegerIntervalModule__, __DeletionHunkModule__) {
    var IHunkModule = __IHunkModule__;

    var IntegerIntervalModule = __IntegerIntervalModule__;

    var DeletionHunkModule = __DeletionHunkModule__;

    (function (Alterity) {
        var InsertionHunk = (function () {
            function InsertionHunk(tick, left, text) {
                this.tick = tick;
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
            InsertionHunk.prototype.ToIntegerInterval = function () {
                return new IntegerIntervalModule.Alterity.IntegerInterval(this.left, this.length);
            };
            InsertionHunk.prototype.ApplyTransformationResults = function (newTick, intervals) {
                return [
                    new InsertionHunk(newTick, intervals[0].left, this.text)
                ];
            };
            InsertionHunk.prototype.MergeSubsequent = function (other) {
                if(other.tick != this.tick) {
                    return other;
                }
                if(other.type == "insertion") {
                    var asInsertion = other;
                    if(asInsertion.left >= this.left && asInsertion.left <= this.left + this.length) {
                        var insertionIndex = asInsertion.left - this.left;
                        var newText = this.text.substr(0, insertionIndex) + asInsertion.text + this.text.substr(insertionIndex);
                        this.text = newText;
                        return null;
                    } else {
                        return asInsertion;
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
                                return new DeletionHunkModule.Alterity.DeletionHunk(this.tick, asDeletion.left, deletionRemainderLength);
                            } else {
                                return null;
                            }
                        }
                    }
                }
            };
            InsertionHunk.prototype.RedoPrior = function (newTick, other) {
                if(other.type == "insertion") {
                    return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().InsertTransformInsertion(other.ToIntegerInterval()));
                } else {
                    if(other.type == "deletion") {
                        return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().DeleteTransformInsertion(other.ToIntegerInterval()));
                    }
                }
            };
            InsertionHunk.prototype.UndoPrior = function (newTick, other) {
                if(other.type == "insertion") {
                    return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().DeleteTransformInsertion(other.ToIntegerInterval()));
                } else {
                    if(other.type == "deletion") {
                        return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().InsertTransformInsertion(other.ToIntegerInterval()));
                    }
                }
            };
            InsertionHunk.prototype.Apply = function (text) {
                return text.substr(0, this.left) + this.text + text.substr(this.left);
            };
            return InsertionHunk;
        })();
        Alterity.InsertionHunk = InsertionHunk;        
    })(exports.Alterity || (exports.Alterity = {}));

})

