define({
    instance: function (node) {
        var _children = new NodeList();
        Object.defineProperty(node, "childNodes", { enumerable: true, get: function () { return _children; } });

        Object.defineProperty(node, "_canHaveChildren", { value: true });
    },
    class: function (nodeClass) {
        Object.defineProperties(nodeClass.prototype, {
            "_insertChildAtIndex": {
                value: function (newChild, index) {
                    if (newChild.ownerDocument != ownerDocument) throw require("DOM").WRONG_DOCUMENT_ERROR;
                    if (newChild._isAncestorOf(this)) throw require("DOM").HIERARCHY_REQUEST_ERROR;
                    if (newChild.nodeType == Node.DOCUMENT_FRAGMENT_NODE) {
                        while (newChild.hasChildren) {
                            _insertChildAtIndex(newChild.firstChild, index);
                            index++;
                        }
                    }
                    else {
                        if (!newChild._canHaveParent) throw require("DOM").HIERARCHY_REQUEST_ERROR;
                        if (newChild.parentNode !== null) newChild.parentNode.removeChild(newChild);
                        childNodes.splice(index, 0, newChild);
                        newChild._setParentNode(this);
                    }
                }
            },
            "appendChild": {
                enumerable: true, value: function (newChild) {
                    _insertChildAtIndex(newChild, childNodes.length);
                    return newChild;
                }
            },
            "_cloneChildNodes": {
                enumerable: false, value: function(document, clone) {
                    for (childNode in childNodes)
                        clone.appendChild(childNode._cloneNodeForImport(document, true));
                }
            },
            "firstChild": { enumerable: true, value: function () { return childNodes.length > 0 ? childNodes[0] : null } },
            "insertBefore": {
                enumerable: true, value: function (newChild, refChild) {
                    var refIndex = childNodes.indexOf(refChild);
                    if (refIndex == -1) throw require("DOM").NOT_FOUND_ERROR;
                    _insertChildAtIndex(newChild, refIndex);
                    return newChild;
                }
            },
            "lastChild": { enumerable: true, get: function () { return childNodes.length > 0 ? childNodes[childNodes.length - 1] : null } },
            "removeChild": {
                enumerable: true, value: function (oldChild) {
                    var oldChildIndex = childNodes.indexOf(oldChild);
                    if (oldChildIndex == -1) throw require("DOM").NOT_FOUND_ERROR;
                    childNodes.splice(oldChildIndex, 1);
                    return oldChild;
                }
            },
            "replaceChild": {
                enumerable: true, value: function (newChild, oldChild) {
                    var oldChildIndex = childNodes.indexOf(oldChild);
                    if (oldChildIndex == -1) throw require("DOM").NOT_FOUND_ERROR;
                    childNodes.splice(oldChildIndex, 1);
                    _insertChildAIndex(newChild, oldChildIndex);
                    return oldChild;
                }
            }
        });
    }
});