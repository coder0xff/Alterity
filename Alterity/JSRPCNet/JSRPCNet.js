var JSRPCNet = function(apiUrl)
{
    var apiObj = this;
    $.ajax({
        cache: false,
        async: false,
        type: "GET",
        url: apiUrl + "/GetApi",
        contentType: 'application/json',
        success: function (response) {
            $.each(response["Methods"], function (key, value)
            {
                var apiMethodName = value.MethodName;

                apiObj[apiMethodName + "Async"] = function()
                {
                    var args = Array.prototype.slice.call(arguments);
                    var callback = args.pop();
                    if (typeof (callback) != "function") throw "The last argument must be a callback function";
                    return JSRPCNet.asyncDispatch(apiUrl, value.MethodIndex, args, callback);
                }

                apiObj[apiMethodName] = function ()
                {
                    var args = Array.prototype.slice.call(arguments);
                    return JSRPCNet.syncDispatch(apiUrl, value.MethodIndex, args);
                }
            });
        }
    });
}

JSRPCNet.asyncDispatch = function(apiUrl, methodIndex, parameterValues, callback) {
    var dispatchData = new Object();
    dispatchData.ParameterValues = parameterValues;
    dispatchData.MethodIndex = methodIndex;
    $.ajax({
        cache: false,
        type: "POST",
        url: apiUrl + "/InvokeApi",
        contentType: 'application/json',
        dataType: "json",
        data: JSON.stringify(dispatchData),
        success: callback
    });
}

JSRPCNet.syncDispatch = function(apiUrl, methodIndex, parameterValues) {
    var result;

    var dispatchData = new Object();
    dispatchData.ParameterValues = parameterValues;
    dispatchData.MethodIndex = methodIndex;
    $.ajax({
        cache: false,
        async: false,
        type: "POST",
        url: apiUrl + "/InvokeApi",
        contentType: 'application/json',
        dataType: "json",
        data: JSON.stringify(dispatchData),
        success: function (receivedData) { result = receivedData; }
    });
    return result;
}
