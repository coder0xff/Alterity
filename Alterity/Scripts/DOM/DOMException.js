define(function () {
    function Exception(code) {
        Object.defineProperty(this, "code", { value: code });
    };

    return Exception;
});