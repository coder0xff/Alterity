require(["IntegerInterval"], function (IntegerInterval, InsertionHunk) {
    var DeletionHunk = function (startIndex, length) {
        this.type = "deletion";
        this.startIndex = startIndex;
        this.length = length;
    };

    DeletionHunk.prototype.getIntegerInterval = function () {
        return new IntegerInterval(this.startIndex, this.length);
    }

    DeletionHunk.prototype.mergeSubsequent = function (other) {
        if (!other) throw "argument null";
        if (other.type == "deletion") {
            //We merge if they are touching because, unlike server side,
            //we only merge with the last queued hunk - so we have more freedom
            if (other.startIndex <= this.startIndex && other.startIndex + other.length >= this.startIndex) {
                this.length += other.length;
                this.startIndex = other.startIndex;
                return null;
            } else {
                return other;
            }
        }
        else if (other.type == "insertion") {
            return other;
        }
        return other;
    }

    return DeletionHunk;
});