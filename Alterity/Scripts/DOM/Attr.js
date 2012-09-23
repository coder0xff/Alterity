define("Node", function (Node) {
    function Attr(ownerDocument, name, specified)
    {
        Node.apply(this, ownerDocument);

        Object.defineProperty(this, "name", { enumerable: true, value: name });

        var _ownerElement;
        Object.defineProperty(this, "ownerElement", { enumerable: true, get: function () { return _ownerElement; } });
        Object.defineProperty(this, "_setOwnerElement", { value: function (ownerElement) { _ownerElement = ownerElement; } });

        Object.defineProperty(this, "specified", { enumerable: true, value: specified });
        
        var _value
        Object.defineProperty(this, "value", { enumerable: true, get: function () { return _value; }, set: function (value) { _value = value; } });
        Object.defineProperty(this, "nodeValue", { enumerable: true, get: function () { return _value; }, set: function (value) { _value = value; } });
    }

    extend(Attr, Node);

    Object.defineProperties(Attr.prototype, {
        "_cloneForParentClone": { value: function () { return new Attr(ownerDocument, name, specified); } },
        "_setParentNode": { value: function (parentNode) { throw "Attributes may not have parent nodes." } },
        "cloneNode": { enumerable: true, value: function (deep) { return new Attr(ownerDocument, name, true); } },
        "nodeName": { enumerable: true, get: function () { return name; } },
        "nodeType": { enumerable: true, value: Node.ATTRIBUTE_NODE },
        "nodeValue": { enumerable: true, get: function () { return value; }, set: function (nodeValue) { value = nodeValue; } }
    });

})