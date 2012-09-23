define(["Node", "enableChildren", "enableParentNode"], function (Node, enableChildren, enableParentNode) {
    function Element(ownerDocument, tagName) {
        if (!Element._validateTagName(tagName)) throw require("DOM").INVALID_CHARACTER_ERR;
        Node.apply(this, ownerDocument);
        enableChildren.instance(this);
        enableParentNode.instance(this);

        Object.defineProperty(this, "tagName", { value: tagName });

        var _attributes = new (Require("NamedNodeMap"))(ownerDocument);
        Object.defineProperty(this, "attributes", { enumerable: true, get: function () { return _attributes; } });
    };

    extend(Element, Node);
    enableChildren.class(Element);

    Element._validateTagName = function (name) {
        var regex = /^[A-Za-z0-9]*$/
    };

    Object.defineProperties(Element.prototype, {
        "_getElementsByTagNameRecursive": {
            value: function (tagName, namedNodeMap) {
                for (childNode in childNodes)
                    if (childNode.nodeType == Node.ELEMENT_NODE) {
                        if (childNode.tagName.toLowerCase() == tagName)
                            result.push(childNode);
                        childNode._getElementsByTagNameRecursive(tagName, namedNodeMap);
                    }
            }
        },
        "getAttribute": { enumerable: true, value: function (name) { var attr = attributes.getNamedItem(name); attr === null ? "" : attr.value; } },
        "getAttributeNode": { enumerable: true, value: function (name) { return attributes.getNamedItem(name); } },
        "getElementsByTagName": {
            enumerable: true, value: function (tagName) {
                tagName = tagName.toLowerCase();
                var result = new (require("NodeList"))();
                _getElementByTagNameRecursive(tagName, result);
                return result;
            }
        },
        "hasAttribute": { enumerable: true, value: function (name) { return attributes.getNamedItem(name) !== null } },
        "removeAttribute": { enumerable: true, value: function (name) { attributes.removeNamedItem(name) } },
        "removeAttributeNode": { enumerable: true, value: function (oldAttr) { removeAttribute(oldAttr.name); } },
        "setAttribute": {
            enumerable: true, value: function (name, value) {
                var attr = getAttribute(name);
                if (attr === null) attr = setAttributeNode(attr = ownerDocument.createAttribute(name))
                attr.value = value;
            }
        },
        "setAttributeNode": {
            enumerable: true, value: function (newAttr) {
                removeAttribute(newAttr.name);
                attributes.setNamedItem(newAttr);
            }
        }
    });

    return Element;
});