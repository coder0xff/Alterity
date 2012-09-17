define(["Node"], function () {
    function Document()
    {
        var rootElement = new Element("html");
        Object.defineProperty(this, "documentElement", { get: function () { return rootElement; } });

        this.createAttribute = function(name)
        {
            //todo
            throw "Not implemented.";
            var Attribute = require("Attribute");
        }

        this.createCDATASection = function(data)
        {
            //todo
            throw "Not implemented.";
            var CDATASection = require("CDATASection");
        }

        this.createComment = function(data)
        {
            //todo
            throw "Not implemented.";
        }

        this.createDocumentFragment = function()
        {
            return new (require("DocumentFragment"))()
        }

        this.createElement = function (tagName) {
            //todo
            throw "Not implemented.";
        };

        this.createEntityReference = function(name)
        {
            //todo
            throw "Not implemented.";
        }

        this.createProcessingInstruction = function(target, data)
        {
            //todo
            throw "Not implemented.";
        }

        this.createTextNode = function(data)
        {
            //todo
            throw "Not implemented.";
        }

        this.getElementById = function(elementId)
        {
            //todo
            throw "Not implemented.";
        }

        this.getElementsByTagName = function(tagName)
        {
            //todo
            throw "Not implemented.";
        }

        this.importNode = function(importedNode, deep)
        {
            //todo
            throw "Not implemented.";
        }
    }

    Document.prototype = new Node;

    Object.defineProperty(Document.prototype, "doctype", { value: null });
    Object.defineProperty(Document.prototype, "implementation", { get: function () { return new (require("DOMImplementation"))(); } });

    return Document;

});