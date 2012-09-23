define({
    instance: function(node)
    {
        var _parentNode = null;
        Object.defineProperty(node, "parentNode", { enumerable: true, get: function () { return parentNode; } });
        Object.defineProperty(node, "_setParentNode", { configurable: true, value: function (node) { _parentNode = node; } });

        Object.defineProperty(node, "_canHaveParent", { value: true });
    }
});