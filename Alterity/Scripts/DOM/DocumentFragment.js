define(["Node"], function () {
    function DocumentFragment() {

    }

    DocumentFragment.prototype = new Node;

    return DocumentFragment;
});