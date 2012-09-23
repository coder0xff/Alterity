﻿define(["Node", "enableChildren"], function (Node, enableChildren) {
    function DocumentFragment(ownerDocument) {
        Node.apply(ownerDocument)
        enableChildren.instance(this);
    };

    extend(DocumentFragment, Node);
    enableChildren.class(DocumentFragment);

    Object.defineProperties(DocumentFragment.prototype, {
        "_setParentNode": { value: function () { throw "DocumentFragment cannot have a parentNode" } },
        "cloneNode": {
            enumerable: true, value: function(deep)
            {
                var clone = ownerDocument.createDocumentFragment();
                //no attributes on a DocumentFragment
                for (childNode in childNodes)
                    clone.appendChild(childNode.cloneNode());
                return clone;
            }
        },
        "nodeName": { enumerable: true, value: "#document-fragment" },
        "nodeType": { enumerable: true, value: Node.DOCUMENT_FRAGMENT_NODE },
        "nodeValue": { enumerable: true, value: null }
    });

    return DocumentFragment;
});