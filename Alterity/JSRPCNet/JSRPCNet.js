var JSRPCNet = function(apiURL)
{
    var apiObj = this;
    $.ajax({
        cache: false,
        async: false,
        type: "GET",
        url: apiURL + "/GetApi",
        contentType: 'application/json',
        success: function (response) {
            $.each(response["Methods"], function (key, value)
            {
                var apiMethodName = value.MethodName;
                if (!apiMethodName.match(/^\w+$/)) { return; } //regex to protect code execution attacks
                for (paramName in value.ParameterNames) {
                    // regex to protect from code execution attacks
                    if (!paramName.match(/^\w+$/)) {
                        return;
                    }
                }

                var FunctionFunctionArguments = value.ParameterNames.slice(0);
                FunctionFunctionArguments.push("JSRPCNetCallback");
                FunctionFunctionArguments.push(" \
                    var args = Array.prototype.slice.call(arguments); \n\
                    args.pop(); \n\
                    JSRPCNet.AsyncDispatch('" + apiURL + "', " + value.MethodIndex + ", arguments, JSRPCNetCallback); \
                    ");
                apiObj[apiMethodName + "Async"] = Function.apply(value, FunctionFunctionArguments)

                FunctionFunctionArguments = value.ParameterNames.slice(0);
                FunctionFunctionArguments.push(" \
                    return JSRPCNet.SyncDispatch('" + apiURL + "', " + value.MethodIndex + ", arguments); \
                    ");

                apiObj[apiMethodName] = Function.apply(value, FunctionFunctionArguments)
            });
        }
    });
}

JSRPCNet.AsyncDispatch = function(apiUrl, methodIndex, parameterValues, callback) {
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

JSRPCNet.SyncDispatch = function(apiUrl, methodIndex, parameterValues) {
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
