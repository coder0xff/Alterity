function require(script) {
    if (!require.loadedScripts) {
        require.loadedScripts = new Array();
    }

    var qualifiedUrl = require.qualifyUrl(script);
    if (require.loadedScripts.indexOf(qualifiedUrl) > -1) return;
    require.loadedScripts.push(qualifiedUrl);

    $.ajax({
        crossDomain: true,
        url: qualifiedUrl,
        dataType: "script",
        async: false,
    });
}

require.qualifyUrl = function (url) {
    var a = document.createElement('a');
    a.href = url;
    return a.href;
}
