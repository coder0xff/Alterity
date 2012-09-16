require(["IntegerInterval"], function(IntegerInterval) {
    var InsertionHunk = function(startIndex, text)
    {
        this.type = "insertion";
        this.startIndex = startIndex;
        this.text = text;
    }

    Object.defineProperty(InsertionHunk.prototype, "length", {
        get: function () { return this.text.length; }
    });

    InsertionHunk.prototype.mergeSubsequent = function (other) {
        if (!other) throw "argument null";
        if (other.startIndex + this.length >= this.startIndex && other.startIndex <= this.startIndex + this.length) {
            if (other.type == "deletion") {
                var resultText = "";
                var remnantIntervals = new IntegerInterval(this.startIndex, this.text.length).Subtract(new IntegerInterval(other.startIndex, other.length));
                for (var remnantIndex = 0; remnantIndex < remnantIntervals.length; remnantIndex++) {
                    var remnantInterval = remnantIntervals[remnantIndex];
                    resultText += this.text.substr(remnantInterval.left - this.startIndex, remnantInterval.length);
                }
                var resultLength = resultText.length;
                var mutualAnnihilationLength = this.text.length - resultLength;
                var deletionRemainderLength = other.length - mutualAnnihilationLength;
                this.text = resultText;
                if (deletionRemainderLength > 0) {
                    return new DeletionHunk(other.startIndex, deletionRemainderLength);
                }
                else {
                    return null;
                }
            }
            else if (other.type == "insertion") {
                var insertionIndex = other.startIndex - this.startIndex;
                this.text = this.text.substr(0, insertionIndex) + other.text + this.text.substr(insertionIndex);
            }
        }
        else
        {
            return other;
        }
    }

    return InsertionHunk;
});