class Node
{
    private _ownerDocument: Document;
   	get ownerDocument() { return this._ownerDocument; };

}    

    function Node(ownerDocument) {
        Object.defineProperty(this, "ownerDocument", { enumerable: true, value: ownerDocument });
    };

    Object.defineProperties(Node.prototype, {
        "_canHaveParent": { value: false },
        "_canHaveChildren": { value: false},
        "_isAncestorOf": {
            value: function (node) {
                var currentAncestor = node.parentNode;
                while (currentAncestor !== null)
                {
                    if (this == currentAncestor) return true;
                    currentAncestor = currentAncestor.parentNode;
                }
                return false;
            }
        },
        "appendChild": { enumerable: true, value: function (newChild) { throw require("DOM").HIERARCHY_REQUEST_ERR; } },
        "attributes": { enumerable: true, value: null },
        "childNodes": { enumerable: true, get: function () { return new (require("NodeList"))(); } },
        "firstChild": { enumerable: true, value: null },
        "hasChildNodes": { enumerable: true, value: function () { return (typeof childNodes == 'undefined') ? false : childNodes.length > 0; } },
        "insertBefore": { enumerable: true, value: function (newChild, refChild) { throw require("DOM").HIERARCHY_REQUEST_ERR; } },
        "lastChild": { enumerable: true, value: null },
        "localName": { enumerable: true, value: null },
        "namespaceURI": { enumerable: true, value: null },
        "nodeValue": { enumerable: true, get: function () { return null; }, set: function (value) { } },
        "nextSibling": {
            enumerable: true, get: function () {
                if (parentNode !== null) {
                    var selfIndex = parentNode.childNodes.indexOf(this);
                    return (selfIndex == parentNode.childNodes.length - 1)
                        ? null
                        : parentNode.childNodes[selfIndex + 1];
                }
                else {
                    return null;
                }
            }
        },
        "normalize": {
            enumerable: true, value: function () {
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
        },
        "parentNode": { enumerable: true, value: null },
        "prefix": { enumerable: true, value: null },
        "previousSibling": {
            enumerable: true, get: function () {
                if (parentNode !== null) {
                    var selfIndex = parentNode.childNodes.indexOf(this);
                    return (selfIndex == 0)
                        ? null
                        : parentNode.childNodes[selfIndex - 1];
                }
                else {
                    return null;
                }
            }
        },
        "removeChild": { enumerable: true, value: function (oldChild) { throw require("DOM").HIERARCHY_REQUEST_ERR; } },
        "replaceChild": { enumerable: true, value: function (newChild, oldChild) { throw require("DOM").HIERARCHY_REQUEST_ERR; } },
        "supports": { enumerable: true, value: function (feature, version) { return false; } }
    });

    Object.defineProperties(Node, {
        "ELEMENT_NODE": { value: 1 },
        "ATTRIBUTE_NODE": { value: 2 },
        "TEXT_NODE": { value: 3 },
        "CDATA_SECTION_NODE": { value: 4 },
        "ENTITY_REFERENCE_NODE": { value: 5 },
        "ENTITY_NODE": { value: 6 },
        "PROCESSING_INSTRUCTION_NODE": { value: 7 },
        "COMMENT_NODE": { value: 8 },
        "DOCUMENT_NODE": { value: 9 },
        "DOCUMENT_TYPE_NODE": { value: 10 },
        "DOCUMENT_FRAGMENT_NODE": { value: 11 },
        "NOTATION_NODE": { value: 12 },
    });