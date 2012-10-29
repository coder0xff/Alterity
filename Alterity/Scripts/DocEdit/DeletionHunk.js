define(["require", "exports", 'IHunk', 'IntegerInterval'], function(require, exports, __IHunkModule__, __IntegerIntervalModule__) {
    var IHunkModule = __IHunkModule__;

    var IntegerIntervalModule = __IntegerIntervalModule__;

    
    (function (Alterity) {
        var DeletionHunk = (function () {
            function DeletionHunk(tick, left, length) {
                this.tick = tick;
                this.left = left;
                this.length = length;
                this.type = "deletion";
            }
            Object.defineProperty(DeletionHunk.prototype, "length", {
                get: function () {
                    return this.length;
                },
                enumerable: true,
                configurable: true
            });
            Object.defineProperty(DeletionHunk.prototype, "right", {
                get: function () {
                    return this.left + this.length - 1;
                },
                enumerable: true,
                configurable: true
            });
            DeletionHunk.prototype.ToIntegerInterval = function () {
                return new IntegerIntervalModule.Alterity.IntegerInterval(this.left, this.length);
            };
            DeletionHunk.prototype.ApplyTransformationResults = function (newTick, intervals) {
                var results = [];
                for(var index = 0; index < intervals.length; index++) {
                    var integerInterval = intervals[index];
                    results.push(new DeletionHunk(newTick, integerInterval.left, integerInterval.length));
                }
                for(var transformeeIndex = 0; transformeeIndex < results.length; transformeeIndex++) {
                    for(var transformerIndex = 0; transformerIndex < transformeeIndex; transformerIndex++) {
                        results[transformeeIndex] = results[transformeeIndex].RedoPrior(results[transformerIndex])[0];
                    }
                }
                return results;
            };
            DeletionHunk.prototype.MergeSubsequent = function (other) {
                if(other.tick != this.tick) {
                    return other;
                }
                if(other.type == "deletion") {
                    var asDeletion = other;
                    if(asDeletion.left <= this.left && asDeletion.left + asDeletion.length >= this.left) {
                        this.length += asDeletion.length;
                        this.left = asDeletion.left;
                        return null;
                    } else {
                        return asDeletion;
                    }
                } else {
                    if(other.type == "insertion") {
                        return other;
                    }
                }
            };
            DeletionHunk.prototype.RedoPrior = function (newTick, other) {
                if(other.type == "insertion") {
                    return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().InsertTransformSelection(other.ToIntegerInterval()));
                } else {
                    if(other.type == "deletion") {
                        return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().DeleteTransformSelection(other.ToIntegerInterval()));
                    }
                }
            };
            DeletionHunk.prototype.UndoPrior = function (newTick, other) {
                if(other.type == "insertion") {
                    return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().DeleteTransformSelection(other.ToIntegerInterval()));
                } else {
                    if(other.type == "deletion") {
                        return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().InsertTransformSelection(other.ToIntegerInterval()));
                    }
                }
            };
            DeletionHunk.prototype.Apply = function (text) {
                return text.substr(0, this.left) + text.substr(this.left + this.length);
            };
            return DeletionHunk;
        })();
        Alterity.DeletionHunk = DeletionHunk;        
    })(exports.Alterity || (exports.Alterity = {}));

})

