import IHunkModule = module('IHunk');
import IntegerIntervalModule = module('IntegerInterval');
import DeletionHunkModule = module('DeletionHunk');

export module Alterity {
    export class InsertionHunk implements IHunkModule.Alterity.IHunk {
        public type = "insertion";
        constructor (public tick: number, public left: number, public text: string) { }
        public get length(): number { return this.text.length; }
        public get right(): number { return this.left + this.length - 1; }
        public ToIntegerInterval(): IntegerIntervalModule.Alterity.IntegerInterval {
            return new IntegerIntervalModule.Alterity.IntegerInterval(this.left, this.length);
        }
        ApplyTransformationResults(newTick: number, intervals: IntegerIntervalModule.Alterity.IntegerInterval[]): IHunkModule.Alterity.IHunk[] {
            return [new InsertionHunk(newTick, intervals[0].left, this.text)];
        }
        public MergeSubsequent(other: IHunkModule.Alterity.IHunk): IHunkModule.Alterity.IHunk {
            if (other.tick != this.tick) return other; //other data has been received from the server between the creation of these hunks
            if (other.type == "insertion") {
                var asInsertion: InsertionHunk = <InsertionHunk>other;
                if (asInsertion.left >= this.left && asInsertion.left <= this.left + this.length) {
                    var insertionIndex = asInsertion.left - this.left;
                    var newText = this.text.substr(0, insertionIndex) + asInsertion.text + this.text.substr(insertionIndex);
                    this.text = newText;
                    return null;
                }
                else {
                    return asInsertion;
                }
            }
            else if (other.type == "deletion") {
                var asDeletion: DeletionHunkModule.Alterity.DeletionHunk = <DeletionHunkModule.Alterity.DeletionHunk>other;
                if (asDeletion.left + this.length >= this.left && asDeletion.left <= this.left + this.length) {
                    var resultText = "";
                    var remnantIntervals = new IntegerIntervalModule.Alterity.IntegerInterval(this.left, this.text.length).Subtract(new IntegerIntervalModule.Alterity.IntegerInterval(asDeletion.left, asDeletion.length));
                    for (var remnantIndex = 0; remnantIndex < remnantIntervals.length; remnantIndex++) {
                        var remnantInterval = remnantIntervals[remnantIndex];
                        resultText += this.text.substr(remnantInterval.left - this.left, remnantInterval.length);
                    }
                    var resultLength = resultText.length;
                    var mutualAnnihilationLength = this.text.length - resultLength;
                    var deletionRemainderLength = asDeletion.length - mutualAnnihilationLength;
                    this.text = resultText;
                    if (deletionRemainderLength > 0) {
                        return new DeletionHunkModule.Alterity.DeletionHunk(this.tick, asDeletion.left, deletionRemainderLength);
                    }
                    else {
                        return null;
                    }
                }
            }
        }
        public RedoPrior(newTick: number, other: IHunkModule.Alterity.IHunk): IHunkModule.Alterity.IHunk[] {
            if (other.type == "insertion") {
                return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().InsertTransformInsertion(other.ToIntegerInterval()));
            }
            else if (other.type == "deletion") {
                return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().DeleteTransformInsertion(other.ToIntegerInterval()));
            }
        }
        public UndoPrior(newTick: number, other: IHunkModule.Alterity.IHunk): IHunkModule.Alterity.IHunk[] {
            if (other.type == "insertion") {
                return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().DeleteTransformInsertion(other.ToIntegerInterval()));
            }
            else if (other.type == "deletion") {
                return this.ApplyTransformationResults(newTick, this.ToIntegerInterval().InsertTransformInsertion(other.ToIntegerInterval()));
            }
        }
        public Apply(text: string): string {
            return text.substr(0, this.left) + this.text + text.substr(this.left);
        }
    }
}