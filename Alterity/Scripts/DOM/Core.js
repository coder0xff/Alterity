define(["DOMException", "DOMImplementation", "DocumentFragment", "NamedNodeMap", "Node", "NodeList"], function () {
    
    var DOM = new Object();

    DOM.DOMException = DOMException

    Object.defineProperties(DOM, {
        "INDEX_SIZE_ERR": { value: 1 },
        "DOMSTRING_SIZE_ERR": { value: 2 },
        "HIERARCHY_REQUEST_ERR": { value: 3 },
        "WRONG_DOCUMENT_ERR": { value: 4 },
        "INVALID_CHARACTER_ERR": { value: 5 },
        "NO_DATA_ALLOWED_ERR": { value: 6 },
        "NO_MODIFICATION_ALLOWED_ERR": { value: 7 },
        "NOT_FOUND_ERR": { value: 8 },
        "NOT_SUPPORTED_ERR": { value: 9 },
        "INUSE_ATTRIBUTE_ERR": { value: 10 },
        "INVALID_STATE_ERR": { value: 11 },
        "SYNTAX_ERR": { value: 12 },
        "INVALID_MODIFICATION_ERR": { value: 13 },
        "NAMESPACE_ERR": { value: 14 },
        "INVALID_ACCESS_ERR": { value: 15 }
    });

    DOM.DOMImplementation = DOMImplementation;

    DOM.DocumenFragment = DocumentFragment;

    return DOM;
});