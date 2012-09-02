require("/Scripts/DocEdit/IntegerInterval.js")
var InsertionHunk = function(startIndex, text)
{
    this.startIndex = startIndex;
    this.text = text;
}

Object.defineProperty(InsertionHunk.prototype, "length", {
    get: function () { return text.length; }
});
