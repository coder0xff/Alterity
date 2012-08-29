var SlimApiDispatch = function(apiMethodURL, parametersArray, callback)
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

var SlimAPI = function(apiURL)
{
    var apiObj = this;
    $.ajax({
        cache: false,
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
                    if (!prop.match(/^\w+$/)) {
                        return;
                    }
                }

                var FunctionFunctionArguments = value.ParameterNames;
                FunctionFunctionArguments.push("slimApiCallback");
                FunctionFunctionArguments.push(" \
                    var args = Array.prototype.slice.call(arguments); \
                    args.pop(); \
                    SlimApiDispatch('" + apiURL + '/' + apiMethodName + ", args, slimApiCallback); \
                    ");
                apiObj[apiMethodName] = Function.apply(value, FunctionFunctionArguments)
            });
        }
    });
}