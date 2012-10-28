define(["Node"], function () {
    function Document() {
        var rootElement = new Element("html");
        Object.defineProperty(this, "documentElement", { get: function () { return rootElement; } });
    };

    extend(Document, Node);

    Object.defineProperties(Document.prototype, {
        "createAttribute": {
            enumerable: true, value: function (name) {
                return new (require("Attribute"))(this, name, true);
            }
        },
        "createCDATASection": {
            enumerable: true, value: function (data) {
                return new (require("CDATASection"))(this, data);
            }
        },
        "createComment": {
            enumerable: true, value: function (data) {
                return new (require("Comment"))(this, data);
            }
        },
        "createDocumentFragment": {
            enumerable: true, value: function () {
                return new (require("DocumentFragment"))()
            }
        },
        "createElement": {
            enumerable: true, value: function (tagName) {
                return new (require("Element"))(this, tagName);
            }
        },
        "createTextNode": {
            enumerable: true, value: function (data) {
                return new (require("Text"))(this, data);
            }
        },
        "getElementById": {
            enumerable: true, value: function (elementId) {
                function getElementByIdRecursive(element, elementId) {
                    if (element.getAttribute("id") == elementId) return element;
                    for (childNode in element.childNodes)
                        if (childNode.nodeType == Node.ELEMENT_NODE) {
                            var recurseResult = getElementByIdRecursive(childNode, elementId);
                            if (recurseResult !== null) return recurseResult;
                        }
                    return null;
                }
                return getElementByIdRecursive(documentElement, elementId);
            }
        },
        "getElementsByTagName": {
            enumerable: true, value: function (tagName) {
                tagName = tagName.toLowerCase();
                var result = new (require("NodeList"))();
                if (documentElement.tagName == tagName) result.push(documentElement);
                documentElement._getElementsByTagNameRecursive(tagName, result);
                return result;
            }
        },
        "importNode": {
            enumerable: true, value: function (importedNode, deep) {
                return importedNode._cloneNodeForImport(this, deep);
            }
        }
    });


    Object.defineProperty(Document.prototype, "doctype", { value: null });
    Object.defineProperty(Document.prototype, "implementation", { get: function () { return new (require("DOMImplementation"))(); } });

    return Document;
});