define("Node", function (Node) {
    function CharacterData(ownerDocument, data)
    {
        var data_storage;

        Object.defineProperty(this, "data", { get: function () { return data_storage; }, set: function (value) { data_storage = "" + value; } });

        this.data = data;
    }

    CharacterData.prototype = new Node;

    Object.defineProperty(CharacterData.prototype, "length", { get: function () { return data.length; } });

    CharacterData.prototype.appendData = function (arg) {
        data += arg;
    }

    CharacterData.prototype.deleteData = function (offset, count) {
        if (offset < 0 || offset > length) throw require("DOM").INDEX_SIZE_ERR;
        if (count < 0) throw require("DOM").INDEX_SIZE_ERR;
        var trailingStart = offset + count;
        data = data.substr(0, offset) + (trailingStart < length ? data.substr(offset + count) : "");
    }

    CharacterData.prototype.insertData = function (offset, arg) {
        if (offset < 0 || offset > length) throw require("DOM").INDEX_SIZE_ERR;
        data = data.substr(0, offset) + arg + data.substr(offset);
    }

    CharacterData.prototype.replaceData = function (offset, count, arg) {
        deleteData(offset, count);
        insertData(offset, arg);
    }

    CharacterData.prototype.substringData = function (offset, count) {
        if (offset < 0 || offset > length) throw require("DOM").INDEX_SIZE_ERR;
        if (count < 0) throw INDEX_SIZE_ERR;
        return data.substr(offset, count);
    }

    return CharacterData;
});