define("CharacterData", function (CharacterData) {
    function Text(ownerDocument, data) {
        CharacterData.apply(this, ownerDocument, data);
    };

    extend(Text, CharacterData);

    Object.defineProperties(Text.prototype, {
        "cloneNode": {
            enumerable: true, value: function (deep) {
                return ownerDocument.createTextNode(data);
            }
        },
        "nodeName": { enumerable: true, get: function () { return "#text"; } },
        "nodeType": { enumerable: true, get: function () { return Node.TEXT_NODE; } },
        "nodeValue": { enumerable: true, get: function () { return data; }, set: function (value) { data = value; } },
        "splitText": {
            enumerable: true, value: function (offset) {
                if (offset < 0 || offset > length) throw require("DOM").INDEX_SIZE_ERR;
                var newNode = ownerDocument.createTextNode(data.substr(offset))
                data = data.substr(0, offset);
                if (parentNode != null)
                    parentNode._insertChildAtIndex(parentNode.childNodes.indexOf(this) + 1, newNode);
                return newNode;
            }
        }
    });

    return Text;
});