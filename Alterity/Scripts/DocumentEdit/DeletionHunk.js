define(["IntegerInterval"], function () {

    var DeletionHunk = function (startIndex, length) {
        this.type = "deletion";
        this.startIndex = startIndex;
        this.length = length;
    };

    DeletionHunk.prototype.getIntegerInterval = function () {
        return new IntegerInterval(startIndex, length);
    }

    DeletionHunk.prototype.MergeSubsequent = function (other) {
        if (!other) throw "argument null";
        if (other.type == "deletion") {
            var otherInterval = other.getIntegerInterval();
            if (otherInterval.left == startIndex) {
                length += otherInterval.length;
                return null;
            } else {
                return other;
            }
        }
        if (other.type == "insertion") return other;
        throw "invalid hunk type";
    }

    return DeletionHunk;
}());