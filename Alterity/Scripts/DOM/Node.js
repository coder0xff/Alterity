define(function () {
    function Node() {

        var _nextSibling = null;
        Object.defineProperty(this, "nextSibling", { get: function () { return _nextSibling; } });
        this._setNextSibling = function (node) { _nextSibling = node; }

        var _ownerDocument = null;
        Object.defineProperty(this, "ownerDocument", { get: function () { return _ownerDocument; } });
        this._setOwnerDocument = function (document) { ownerDocument = document; }

        var _parentNode = null;
        Object.defineProperty(this, "parentNode", { get: function () { return parentNode; } });
        this._setParentNode = function (node) { _parentNode = node; }

        var _previousSibling = null;
        Object.defineProperty(this, "previousSibling", { get: function () { return previousSibling; } });
        this._setPreviousSibling = function (node) { _previousSibling = node; }

    }

    Object.defineProperty(Node.prototype, "attributes", { value: null });

    Object.defineProperty(Node.prototype, "childNodes", { get: function () { return new (require("NodeList"))(); } });

    Object.defineProperty(Node.prototype, "firstChild", { value: null });

    Object.defineProperty(Node.prototype, "lastChild", { value: null });

    Object.defineProperty(Node.prototype, "nodeName", { get: function () { throw "Not implemented by inheritor!"; } });

    Object.defineProperty(Node.prototype, "nodeType", { get: function () { throw "Not implemented by inheritor!"; } });

    Object.defineProperty(Node.prototype, "nodeValue", { get: function () { return null; }, set: function (value) { } });

    Object.defineProperty(Node.prototype, "localName", { value: null });

    Object.defineProperty(Node.prototype, "namespaceURI", { value: null });

    Object.defineProperty(Node.prototype, "prefix", { value: null });

    Object.prototype.appendChild = function (newChild) { throw HIERARCHY_REQUEST_ERR; }
        
    Object.prototype.clone = function () { throw "Not implemented by inheritor!"; }

    Object.prototype.hasChildNodes = function () { return false; }

    Object.prototype.insertBefore = function (newChild, refChild) { throw HIERARCHY_REQUEST_ERR; }

    Object.prototype.normalize = function () {
        var previousWasText = false;
        for (var i = 0; i < childNodes.length; i++) {
            if (childNodes[i].nodeType == Node.TEXT_NODE) {
                if (previousWasText) {
                    replaceChild(ownerDocument.createTextNode(childNodes[i - 1].nodeValue + childNodes[i].nodeValue), childNodes[i - 1]);
                    i--;
                } else {
                    previousWasText = true;
                }
            } else {
                childNodes[i].normalize();
                previousWasText = false;
            }
        }

        if (attributes !== null) {
            for (var i = 0; i < attributes.length; i++) {
                attributes[i].normalize();
            }
        }
    }

    Object.prototype.removeChild = function (oldChild) { throw HIERARCHY_REQUEST_ERR; }

    Object.prototype.replaceChild = function (newChild, oldChild) { throw HIERARCHY_REQUEST_ERR; }

    Object.prototype.supports = function (feature, version) { return false; }

    Object.defineProperties(Node, {
        "ELEMENT_NODE": { value: 1 },
        "ATTRIBUTE_NODE": { value: 1 },
        "TEXT_NODE": { value: 1 },
        "CDATA_SECTION_NODE": { value: 1 },
        "ENTITY_REFERENCE_NODE": { value: 1 },
        "ENTITY_NODE": { value: 1 },
        "PROCESSING_INSTRUCTION_NODE": { value: 1 },
        "COMMENT_NODE": { value: 1 },
        "DOCUMENT_NODE": { value: 1 },
        "DOCUMENT_TYPE_NODE": { value: 1 },
        "DOCUMENT_FRAGMENT_NODE": { value: 1 },
        "NOTATION_NODE": { value: 1 },
    });

    return Node;
});