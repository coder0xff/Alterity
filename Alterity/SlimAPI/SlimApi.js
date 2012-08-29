var SlimApiArgumentsObjectToObject= function(argumentsObject, parametersArray)
{
    var results = new Object();
    for (i = 0; i < parametersArray.length; i++)
        results[parametersArray[i]] = argumentsObject[i]
    return results;
}

var SlimApiAsyncDispatch = function (apiURL, methodIndex, parametersArray, callback)
{
    parametersArray["SlimApiMethodIndex"] = methodIndex;
    $.ajax({
        cache: false,
        type: "POST",
        url: apiUrl + "/InvokeApi",
        contentType: 'application/json',
        dataType: "json",
        data: JSON.stringify(parametersArray),
        success: callback
    });
}

var SlimApiSyncDispatch = function(apiUrl, methodIndex, parametersArray)
{
    var result;
    parametersArray["SlimApiMethodIndex"] = methodIndex;
    var jsonString = JSON.stringify(parametersArray);
    $.ajax({
        cache: false,
        async: false,
        type: "POST",
        url: apiUrl + "/InvokeApi",
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
                    var argNames = eval('" + JSON.stringify(value.ParameterNames) + "'); \
                    var args = SlimApiArgumentsObjectToObject(arguments, argNames); \
                    args.pop(); \
                    SlimApiAsyncDispatch('" + apiURL + "', " + value.MethodIndex + ", args, slimApiCallback); \
                    ");
                apiObj[apiMethodName + "Async"] = Function.apply(value, FunctionFunctionArguments)

                FunctionFunctionArguments = value.ParameterNames.slice(0);
                FunctionFunctionArguments.push(" \
                    var argNames = eval('" + JSON.stringify(value.ParameterNames) + "'); \
                    var args = SlimApiArgumentsObjectToObject(arguments, argNames); \
                    return SlimApiSyncDispatch('" + apiURL + "', " + value.MethodIndex + ", args); \
                    ");

                apiObj[apiMethodName] = Function.apply(value, FunctionFunctionArguments)
            });
        }
    });
}