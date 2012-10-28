export module Alterity {
    export class IntegerInterval {
        constructor (public left: number = 0, public length: number = 0) { }
        public get right() { return this.left + this.length - 1; }
        public set right(value: number) { this.length = value - this.left + 1; }

        public Clone(): IntegerInterval { return new IntegerInterval(this.left, this.length); }

        public InsertTransformSelection(transformer: IntegerInterval): IntegerInterval[] {
            if (transformer.left <= this.left)
                return [new IntegerInterval(this.left + transformer.length, this.length)];
            else if (transformer.left < this.left + this.length)
                return [new IntegerInterval(this.left, transformer.left - this.left),
                    new IntegerInterval(transformer.left + transformer.length, this.length - (transformer.left - this.left))];
            else
                return [this.Clone()];
        }

        public DeleteTransformSelection(asInsertion: IntegerInterval): IntegerInterval[] {
            var leftShiftCount = Math.max(0, Math.min(this.left - asInsertion.left, asInsertion.length));
            var upperBoundMin = Math.min(this.left + this.length - 1, asInsertion.left + asInsertion.length - 1);
            var lowerBoundMax = Math.max(this.left, asInsertion.left);
            var lengthReduction = (lowerBoundMax <= upperBoundMin) ? Math.max(upperBoundMin - lowerBoundMax + 1, 0) : 0;
            var newLength = this.length - lengthReduction;
            if (newLength > 0)
                return [new IntegerInterval(this.left - leftShiftCount, newLength)];
            else
                return [];
        }

        public InsertTransformInsertion(asInsertion: IntegerInterval): IntegerInterval[] {
            if (asInsertion.left <= this.left)
                return [new IntegerInterval(this.left + asInsertion.length, this.length)];
            else
                return [this.Clone()];
        }

        public DeleteTransformInsertion(asDeletion: IntegerInterval): IntegerInterval[] {
            var leftShift = Math.max(0, Math.min(this.left - asDeletion.left, asDeletion.length));
            return [new IntegerInterval(this.left - leftShift, this.length)];
        }

        public InsertTransformInsertionSwappedPrecendence(asInsertion: IntegerInterval): IntegerInterval[] {
            if (asInsertion.left < this.left)
                return [new IntegerInterval(this.left + asInsertion.length, this.length)];
            else
                return [this.Clone()];
        }

        public Intersection(other: IntegerInterval): IntegerInterval {
            var result = new IntegerInterval();
            if (other.right >= this.left && other.left <= this.right) {
                result.left = Math.max(other.left, this.left);
                result.length = Math.min(other.right, this.right) - result.left + 1;
            }
            else {
                result.left = 0;
                result.length = 0;
            }
            return result;
        }

        public Union(other: IntegerInterval): IntegerInterval {
            var result = new IntegerInterval();
            if (other.right >= this.left && other.left <= this.right) {
                result.left = Math.min(other.left, this.left);
                result.length = Math.max(other.right, this.right) - result.left + 1;
            }
            else {
                result.left = 0;
                result.length = 0;
            }
            return result;
        }

        public Subtract(other: IntegerInterval): IntegerInterval[] {
            if (other.right >= this.left && other.left <= this.right) {
                var leftResultLength = Math.max(other.left - this.left, 0);
                var rightResultLength = Math.max(this.right - other.right, 0);
                if (leftResultLength + rightResultLength < this.length) {
                    if (leftResultLength > 0) {
                        if (rightResultLength > 0) {
                            return [new IntegerInterval(this.left, leftResultLength), new IntegerInterval(this.right - rightResultLength + 1, rightResultLength)];
                        }
                        else {
                            return [new IntegerInterval(this.left, leftResultLength)];
                        }
                    }
                    else {
                        if (rightResultLength > 0) {
                            return [new IntegerInterval(this.right - rightResultLength + 1, rightResultLength)];
                        }
                        else {
                            return [];
                        }
                    }
                }
            }
            return [this.Clone()];
        }
    }
}