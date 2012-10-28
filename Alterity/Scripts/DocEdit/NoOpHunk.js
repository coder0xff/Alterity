define(["require", "exports", 'IHunk', 'IntegerInterval'], function(require, exports, __IHunkModule__, __IntegerIntervalModule__) {
    var IHunkModule = __IHunkModule__;

    var IntegerIntervalModule = __IntegerIntervalModule__;

    (function (Alterity) {
        var NoOpHunk = (function () {
            function NoOpHunk(tick, left, length) {
                this.tick = tick;
                this.left = left;
                this.length = length;
                this.type = "deletion";
            }
            Object.defineProperty(NoOpHunk.prototype, "length", {
                get: function () {
                    return this.length;
                },
                enumerable: true,
                configurable: true
            });
            Object.defineProperty(NoOpHunk.prototype, "right", {
                get: function () {
                    return this.left + this.length - 1;
                },
                enumerable: true,
                configurable: true
            });
            NoOpHunk.prototype.ToIntegerInterval = function () {
                return new IntegerIntervalModule.Alterity.IntegerInterval(this.left, this.length);
            };
            NoOpHunk.prototype.ApplyTransformationResults = function (newTick, intervals) {
                var results = new ();
                for(var index = 0; index < intervals.length; index++) {
                    var integerInterval = intervals[index];
                    results.push(new NoOpHunk(newTick, integerInterval.left, integerInterval.length));
                }
                for(var transformeeIndex = 0; transformeeIndex < results.length; transformeeIndex++) {
                    for(var transformerIndex = 0; transformerIndex < transformeeIndex; transformerIndex++) {
                        results[transformeeIndex] = results[transformeeIndex].RedoPrior(results[transformerIndex])[0];
                    }
                }
                return results;
            };
            NoOpHunk.prototype.MergeSubsequent = function (other) {
                return other;
            };
            NoOpHunk.prototype.RedoPrior = function (newTick, other) {
                if(other.type == "insertion") {
                    return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().InsertTransformSelection(other.ToIntegerInterval()));
                } else {
                    if(other.type == "deletion") {
                        return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().DeleteTransformSelection(other.ToIntegerInterval()));
                    }
                }
            };
            NoOpHunk.prototype.UndoPrior = function (newTick, other) {
                if(other.type == "insertion") {
                    return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().DeleteTransformSelection(other.ToIntegerInterval()));
                } else {
                    if(other.type == "deletion") {
                        return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().InsertTransformSelection(other.ToIntegerInterval()));
                    }
                }
            };
            NoOpHunk.prototype.Apply = function (text) {
                return text.substr(0, this.left) + text.substr(this.left + this.length);
            };
            return NoOpHunk;
        })();
        Alterity.NoOpHunk = NoOpHunk;        
    })(exports.Alterity || (exports.Alterity = {}));

})

