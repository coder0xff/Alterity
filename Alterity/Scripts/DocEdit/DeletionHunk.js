define(["require", "exports", 'IHunk', 'InsertionHunk'], function(require, exports, __IHunkModule__, __InsertionHunkModule__) {
    var IHunkModule = __IHunkModule__;

    
    var InsertionHunkModule = __InsertionHunkModule__;

    (function (Alterity) {
        var DeletionHunk = (function () {
            function DeletionHunk(left, length) {
                this.left = left;
                this.length = length;
                this.type = "deletion";
            }
            Object.defineProperty(DeletionHunk.prototype, "length", {
                get: function () {
                    return this.length;
                },
                enumerable: true,
                configurable: true
            });
            Object.defineProperty(DeletionHunk.prototype, "right", {
                get: function () {
                    return this.left + this.length - 1;
                },
                enumerable: true,
                configurable: true
            });
            DeletionHunk.prototype.MergeSubsequent = function (other) {
                if(other.type == "deletion") {
                    var asDeletion = other;
                    if(asDeletion.left <= this.left && asDeletion.left + asDeletion.length >= this.left) {
                        this.length += asDeletion.length;
                        this.left = asDeletion.left;
                        return null;
                    } else {
                        return new DeletionHunk(asDeletion.left, asDeletion.length);
                    }
                } else {
                    if(other.type == "insertion") {
                        var asInsertion = other;
                        return new InsertionHunkModule.Alterity.InsertionHunk(asInsertion.left, asInsertion.text);
                    }
                }
            };
            return DeletionHunk;
        })();
        Alterity.DeletionHunk = DeletionHunk;        
    })(exports.Alterity || (exports.Alterity = {}));

})

