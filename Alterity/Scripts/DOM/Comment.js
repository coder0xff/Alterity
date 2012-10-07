define("CharacterData", function (CharacterData) {
    function Comment(ownerDocument, data) {
        CharacterData.apply(this, ownerDocument, data);
    };

    extend(Comment, CharacterData);

    Object.defineProperties(Comment.prototype, {
        "_cloneNodeForImport": {
            value: function (document, deep) {
                return new Comment(document, data);
            }
        },
        "cloneNode": {
            enumerable: true, value: function (deep) {
                return ownerDocument.createComment(data);
            }
        },
        "nodeName": { enumerable: true, get: function () { return "#comment"; } },
        "nodeType": { enumerable: true, get: function () { return Node.COMMENT_NODE; } },
        "nodeValue": { enumerable: true, get: function () { return data; }, set: function (value) { data = value; } },
    });

    return Comment;
});