var SlimApiAsyncDispatch = function(apiURL, methodIndex, parametersArray, callback)
{
    $.ajax({
        cache: false,
        type: "POST",
        url: apiMethodURL,
        contentType: 'application/json',
        dataType: "json",
        data: JSON.stringify(parametersArray),
        success: callback
    });
}

var SlimApiSyncDispatch = function(apiUrl, methodIndex, parametersArray)
{
    var result;
    $.ajax({
        cache: false,
        async: false,
        type: "POST",
        url: apiMethodURL,
        contentType: 'application/json',
        dataType: "json",
        data: JSON.stringify(parametersArray),
        success: function (receivedData) { result = receivedData; }
    });
    return result;
}

var SlimAPI = function(apiURL)
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
                FunctionFunctionArguments.push("slimApiCallback");
                FunctionFunctionArguments.push(" \
                    var args = Array.prototype.slice.call(arguments); \
                    args.pop(); \
                    SlimApiAsyncDispatch('" + apiURL + '/' + apiMethodName + "', args, slimApiCallback); \
                    ");
                apiObj[apiMethodName + "Async"] = Function.apply(value, FunctionFunctionArguments)

                FunctionFunctionArguments = value.ParameterNames.slice(0);
                FunctionFunctionArguments.push(" \
                    var args = Array.prototype.slice.call(arguments); \
                    return SlimApiSyncDispatch('" + apiURL + '/' + apiMethodName + "', args); \
                    ");

                apiObj[apiMethodName] = Function.apply(value, FunctionFunctionArguments)
            });
        }
    });
}