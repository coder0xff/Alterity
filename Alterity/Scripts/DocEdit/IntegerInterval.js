define(["require", "exports"], function(require, exports) {
    (function (Alterity) {
        var IntegerInterval = (function () {
            function IntegerInterval(left, length) {
                if (typeof left === "undefined") { left = 0; }
                if (typeof length === "undefined") { length = 0; }
                this.left = left;
                this.length = length;
            }
            Object.defineProperty(IntegerInterval.prototype, "right", {
                get: function () {
                    return this.left + this.length - 1;
                },
                set: function (value) {
                    this.length = value - this.left + 1;
                },
                enumerable: true,
                configurable: true
            });
            IntegerInterval.prototype.Clone = function () {
                return new IntegerInterval(this.left, this.length);
            };
            IntegerInterval.prototype.InsertTransformSelection = function (transformer) {
                if(transformer.left <= this.left) {
                    return [
                        new IntegerInterval(this.left + transformer.length, this.length)
                    ];
                } else {
                    if(transformer.left < this.left + this.length) {
                        return [
                            new IntegerInterval(this.left, transformer.left - this.left), 
                            new IntegerInterval(transformer.left + transformer.length, this.length - (transformer.left - this.left))
                        ];
                    } else {
                        return [
                            this.Clone()
                        ];
                    }
                }
            };
            IntegerInterval.prototype.DeleteTransformSelection = function (asInsertion) {
                var leftShiftCount = Math.max(0, Math.min(this.left - asInsertion.left, asInsertion.length));
                var upperBoundMin = Math.min(this.left + this.length - 1, asInsertion.left + asInsertion.length - 1);
                var lowerBoundMax = Math.max(this.left, asInsertion.left);
                var lengthReduction = (lowerBoundMax <= upperBoundMin) ? Math.max(upperBoundMin - lowerBoundMax + 1, 0) : 0;
                var newLength = this.length - lengthReduction;
                if(newLength > 0) {
                    return [
                        new IntegerInterval(this.left - leftShiftCount, newLength)
                    ];
                } else {
                    return [];
                }
            };
            IntegerInterval.prototype.InsertTransformInsertion = function (asInsertion) {
                if(asInsertion.left <= this.left) {
                    return [
                        new IntegerInterval(this.left + asInsertion.length, this.length)
                    ];
                } else {
                    return [
                        this.Clone()
                    ];
                }
            };
            IntegerInterval.prototype.DeleteTransformInsertion = function (asDeletion) {
                var leftShift = Math.max(0, Math.min(this.left - asDeletion.left, asDeletion.length));
                return [
                    new IntegerInterval(this.left - leftShift, this.length)
                ];
            };
            IntegerInterval.prototype.InsertTransformInsertionSwappedPrecendence = function (asInsertion) {
                if(asInsertion.left < this.left) {
                    return [
                        new IntegerInterval(this.left + asInsertion.length, this.length)
                    ];
                } else {
                    return [
                        this.Clone()
                    ];
                }
            };
            IntegerInterval.prototype.Intersection = function (other) {
                var result = new IntegerInterval();
                if(other.right >= this.left && other.left <= this.right) {
                    result.left = Math.max(other.left, this.left);
                    result.length = Math.min(other.right, this.right) - result.left + 1;
                } else {
                    result.left = 0;
                    result.length = 0;
                }
                return result;
            };
            IntegerInterval.prototype.Union = function (other) {
                var result = new IntegerInterval();
                if(other.right >= this.left && other.left <= this.right) {
                    result.left = Math.min(other.left, this.left);
                    result.length = Math.max(other.right, this.right) - result.left + 1;
                } else {
                    result.left = 0;
                    result.length = 0;
                }
                return result;
            };
            IntegerInterval.prototype.Subtract = function (other) {
                if(other.right >= this.left && other.left <= this.right) {
                    var leftResultLength = Math.max(other.left - this.left, 0);
                    var rightResultLength = Math.max(this.right - other.right, 0);
                    if(leftResultLength + rightResultLength < this.length) {
                        if(leftResultLength > 0) {
                            if(rightResultLength > 0) {
                                return [
                                    new IntegerInterval(this.left, leftResultLength), 
                                    new IntegerInterval(this.right - rightResultLength + 1, rightResultLength)
                                ];
                            } else {
                                return [
                                    new IntegerInterval(this.left, leftResultLength)
                                ];
                            }
                        } else {
                            if(rightResultLength > 0) {
                                return [
                                    new IntegerInterval(this.right - rightResultLength + 1, rightResultLength)
                                ];
                            } else {
                                return [];
                            }
                        }
                    }
                }
                return [
                    this.Clone()
                ];
            };
            return IntegerInterval;
        })();
        Alterity.IntegerInterval = IntegerInterval;        
    })(exports.Alterity || (exports.Alterity = {}));

})

