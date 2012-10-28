import IntegerIntervalModule = module("IntegerInterval");

export module Alterity {
    export interface IHunk {
        //"insertion" of "deletion"
        type: string;
        //the server tick that the client is currently operating under
        tick: number;
        ToIntegerInterval(): IntegerIntervalModule.Alterity.IntegerInterval;
        MergeSubsequent(other: IHunk): IHunk;
        RedoPrior(newTick: number, other: IHunk): IHunk[];
        UndoPrior(newTick: number, other: IHunk): IHunk[];
        Apply(text: string): string;
    }
}