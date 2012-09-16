define(function () {
    function NamedNodeMap()
    { }

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
                if (keyName.toLowerCase() == name) delete this[keyName];
            }
        }
    });

    Object.defineProperty(NamedNodeMap.prototype, "setNamedItem", {
        value: function (node) {
            var nodeName = node.nodeName.toLowerCase();
            this[nodeName] = node;
        }
    });

    return NamedNodeMap;
});