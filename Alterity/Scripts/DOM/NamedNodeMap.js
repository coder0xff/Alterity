define(function () {
    function NamedNodeMap(ownerDocument)
    {
        Object.defineProperty(this, "ownerDocument", { value: ownerDocument });
    }

    Object.defineProperty(NamedNodeMap.prototype, "getNamedItem", {
        value: function (name) {
            name = name.toLowerCase();
            for (var keyName in this)
            {
                if (keyName.toLowerCase() == name) return this[keyName];
            }
            return null;
        }
    });

    Object.defineProperty(NamedNodeMap.prototype, "item", {
        value: function (index) {
            return this[Object.keys(this)[index]];
        }
    });

    Object.defineProperty(NamedNodeMap.prototype, "length", {
        get: function () {
            return Object.keys(this).length;
        }
    });

    Object.defineProperty(NamedNodeMap.prototype, "removeNamedItem", {
        value: function (name) {
            name = name.toLowerCase();
            for (var keyName in this) {
                if (keyName.toLowerCase() == name) {
                    var node = this[keyName]
                    node._setOwnerElement(null);
                    delete this[keyName];
                    return node;
                }
            }
            throw require("DOM").NOT_FOUND_ERR
        }
    });

    Object.defineProperty(NamedNodeMap.prototype, "setNamedItem", {
        value: function (node) {
            if (node.ownerDocument != ownerDocument) throw require("DOM").WRONG_DOCUMENT_ERROR;
            if (node.ownerElement !== null) throw require("DOM").INUSE_ATTRIBUTE_ERR;
            var nodeName = node.nodeName.toLowerCase();
            removeNamedItem(nodeName);
            this[nodeName] = node;
            node._setOwnerElement(this);
            return node;
        }
    });

    return NamedNodeMap;
});