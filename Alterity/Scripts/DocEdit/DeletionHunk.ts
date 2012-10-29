import IHunkModule = module('IHunk');
import IntegerIntervalModule = module('IntegerInterval');
import InsertionHunkModule = module('InsertionHunk');

export module Alterity {
    export class DeletionHunk implements IHunkModule.Alterity.IHunk {
        public type = "deletion";
        constructor (public tick: number, public left: number, public length: number) { }
        public get length(): number { return this.length; }
        public get right(): number { return this.left + this.length - 1; }
        public ToIntegerInterval(): IntegerIntervalModule.Alterity.IntegerInterval {
            return new IntegerIntervalModule.Alterity.IntegerInterval(this.left, this.length);
        }
        ApplyTransformationResults(newTick: number, intervals: IntegerIntervalModule.Alterity.IntegerInterval[]): IHunkModule.Alterity.IHunk[] {
            var results = [];
            for (var index = 0; index < intervals.length; index++) {
                var integerInterval = intervals[index];
                results.push(new DeletionHunk(newTick, integerInterval.left, integerInterval.length));
            }
            for (var transformeeIndex = 0; transformeeIndex < results.length; transformeeIndex++) {
                for (var transformerIndex = 0; transformerIndex < transformeeIndex; transformerIndex++) {
                    results[transformeeIndex] = results[transformeeIndex].RedoPrior(results[transformerIndex])[0];
                }
            }
            return results;
        }
        public MergeSubsequent(other: IHunkModule.Alterity.IHunk): IHunkModule.Alterity.IHunk {
            if (other.tick != this.tick) return other; //other data has been received from the server between the creation of these hunks
            if (other.type == "deletion") {
                var asDeletion: DeletionHunk = <DeletionHunk>other;
                if (asDeletion.left <= this.left && asDeletion.left + asDeletion.length >= this.left) {
                    this.length += asDeletion.length;
                    this.left = asDeletion.left;
                    return null;
                } else {
                    return asDeletion;
                }
            }
            else if (other.type == "insertion") {
                return other;
            }
        }
        public RedoPrior(newTick: number, other: IHunkModule.Alterity.IHunk): IHunkModule.Alterity.IHunk[] {
            if (other.type == "insertion") {
                return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().InsertTransformSelection(other.ToIntegerInterval()));
            }
            else if (other.type == "deletion") {
                return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().DeleteTransformSelection(other.ToIntegerInterval()));
            }
        }
        public UndoPrior(newTick: number, other: IHunkModule.Alterity.IHunk): IHunkModule.Alterity.IHunk[] {
            if (other.type == "insertion") {
                return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().DeleteTransformSelection(other.ToIntegerInterval()));
            }
            else if (other.type == "deletion") {
                return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().InsertTransformSelection(other.ToIntegerInterval()));
            }
        }
        public Apply(text: string): string {
            return text.substr(0, this.left) + text.substr(this.left + this.length);
        }
    }
}