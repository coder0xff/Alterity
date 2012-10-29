import IHunkModule = module('IHunk');
import IntegerIntervalModule = module('IntegerInterval');

export module Alterity {
    export class NoOpHunk implements IHunkModule.Alterity.IHunk {
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
                results.push(new NoOpHunk(newTick, integerInterval.left, integerInterval.length));
            }
            for (var transformeeIndex = 0; transformeeIndex < results.length; transformeeIndex++) {
                for (var transformerIndex = 0; transformerIndex < transformeeIndex; transformerIndex++) {
                    results[transformeeIndex] = results[transformeeIndex].RedoPrior(results[transformerIndex])[0];
                }
            }
            return results;
        }
        public MergeSubsequent(other: IHunkModule.Alterity.IHunk): IHunkModule.Alterity.IHunk {
            return other;
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