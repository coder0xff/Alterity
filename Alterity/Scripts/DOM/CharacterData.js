define(["Node", "enableParenNode"], function (Node, enableParentNode) {
    function CharacterData(ownerDocument, data) {
        Node.apply(this, ownerDocument);
        enableParentNode.instance(this);

        var data_storage;
        Object.defineProperty(this, "data", { enumerable: true, get: function () { return data_storage; }, set: function (value) { data_storage = "" + value; } });
    };

    extend(CharacterData, Node);

    Object.defineProperties(CharacterData.prototype, {
        "appendData": { enumerable: true, value: function (arg) { data += arg; } },
        "cloneNode": {
            enumerable: true, value: function () { return new CharacterData(ownerDocument, data); }
        },
        "deleteData": {
            enumerable: true, value: function (offset, count) {
                if (offset < 0 || offset > length) throw require("DOM").INDEX_SIZE_ERR;
                if (count < 0) throw require("DOM").INDEX_SIZE_ERR;
                var trailingStart = offset + count;
                data = data.substr(0, offset) + (trailingStart < length ? data.substr(offset + count) : "");
            }
        },
        "insertData": {
            enumerable: true, value: function (offset, arg) {
                if (offset < 0 || offset > length) throw require("DOM").INDEX_SIZE_ERR;
                data = data.substr(0, offset) + arg + data.substr(offset);
            }
        },
        "length": { enumerable: true, get: function () { return data.length; } },
        "nodeName": { enumerable: true, value: "#cdata-section" },
        "nodeType": { enumerable: true, value: Node.CDATA_SECTION_NODE },
        "nodeValue": { enumerable: true, get: function () { return data; } },
        "replaceData": {
            enumerable: true, value: function (offset, count, arg) {
                deleteData(offset, count);
                insertData(offset, arg);
            }
        },
        "substringData": {
            enumerable: true, value: function (offset, count) {
                if (offset < 0 || offset > length) throw require("DOM").INDEX_SIZE_ERR;
                if (count < 0) throw require("DOM").INDEX_SIZE_ERR;
                return data.substr(offset, count);
            }
        }
    });

    return CharacterData;
});