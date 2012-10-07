define("Text", function (Text) {
    function CDATASection(ownerDocument, data) {
        Text.apply(this, ownerDocument, data);
    };

    extend(CDATASection, Text);

    Object.defineProperties(CDATASection.prototype, {
        "_cloneNodeForImport": {
            value: function(document, deep) {
                return new CDATASection(document, data);
            }
        },
        "cloneNode": {
            enumerable: true, value: function (deep) {
                return new CDATASection(ownerDocument, data);
            }
        },
        "nodeName": { enumerable: true, get: function () { return "#cdata-section"; } },
        "nodeType": { enumerable: true, get: function () { return Node.CDATA_SECTION_NODE; } },
        "nodeValue": { enumerable: true, get: function () { return data; }, set: function (value) { data = value; } },
    });

    return CDATASection;
});