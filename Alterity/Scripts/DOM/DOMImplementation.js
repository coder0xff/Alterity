define(function () {
    var instance = null;
    function DOMImplementation() {
        if (instance != null) return instance;
        this.hasFeature = function (feature, version) {
            return false;
        }
    };

    return DOMImplementation;
});