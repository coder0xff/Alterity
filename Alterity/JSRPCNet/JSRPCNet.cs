using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using System.Linq.Expressions;

namespace JSRPCNet
{
    internal static class DelegateFactory
    {
        public static Func<Object, Object[], Object> CreateForNonVoidInstanceMethod(MethodInfo method)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(Object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(Object[]), "arguments");

            MethodCallExpression call = Expression.Call(
              Expression.Convert(instanceParameter, method.DeclaringType),
              method,
              CreateParameterExpressions(method, argumentsParameter));

            Expression<Func<Object, Object[], Object>> lambda = Expression.Lambda<Func<Object, Object[], Object>>(
              Expression.Convert(call, typeof(Object)),
              instanceParameter,
              argumentsParameter);

            return lambda.Compile();
        }

        private static Func<Object[], Object> CreateForNonVoidStaticMethod(MethodInfo method)
        {
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(Object[]), "arguments");

            MethodCallExpression call = Expression.Call(
              method,
              CreateParameterExpressions(method, argumentsParameter));

            Expression<Func<Object[], Object>> lambda = Expression.Lambda<Func<Object[], Object>>(
              Expression.Convert(call, typeof(Object)),
              argumentsParameter);

            return lambda.Compile();
        }

        public static Action<Object, Object[]> CreateForVoidInstanceMethod(MethodInfo method)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(Object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(Object[]), "arguments");

            MethodCallExpression call = Expression.Call(
              Expression.Convert(instanceParameter, method.DeclaringType),
              method,
              CreateParameterExpressions(method, argumentsParameter));

            Expression<Action<Object, Object[]>> lambda = Expression.Lambda<Action<Object, Object[]>>(
              call,
              instanceParameter,
              argumentsParameter);

            return lambda.Compile();
        }

        private static Action<Object[]> CreateForVoidStaticMethod(MethodInfo method)
        {
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(Object[]), "arguments");

            MethodCallExpression call = Expression.Call(
              method,
              CreateParameterExpressions(method, argumentsParameter));

            Expression<Action<Object[]>> lambda = Expression.Lambda<Action<Object[]>>(
              call,
              argumentsParameter);

            return lambda.Compile();
        }

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
        {
            return method.GetParameters().Select((parameter, index) =>
              Expression.Convert(
                Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType)).ToArray();
        }

        public static Func<Object, Object[], Object> Bind(this MethodInfo method)
        {
            if (method.IsStatic)
            {
                if (method.ReturnType == typeof(void))
                {
                    var wrapped = CreateForVoidStaticMethod(method);
                    return (Object target, Object[] parameters) => { wrapped(parameters); return (Object)null; };
                }
                else
                {
                    var wrapped = CreateForNonVoidStaticMethod(method);
                    return (Object target, Object[] parameters) => { return wrapped(parameters); };
                }
            }
            else
            {
                if (method.ReturnType == typeof(void))
                {
                    var wrapped = CreateForVoidInstanceMethod(method);
                    return (Object target, Object[] parameters) => { wrapped(target, parameters); return (Object)null; };
                }
                else
                {
                    var wrapped = CreateForNonVoidInstanceMethod(method);
                    return (Object target, Object[] parameters) => { return wrapped(target, parameters); };
                }
            }
        }
    }

    internal class ApiMethodInfo
    {
        public string MethodName { get; set; }
        public int MethodIndex { get; set; }

        internal List<Type> ParameterTypes { get; set; }
        internal Func<Object, Object[], Object> fastCall;

        public ApiMethodInfo(MethodInfo method, int methodIndex)
        {
            MethodName = method.Name;
            MethodIndex = methodIndex;
            ParameterTypes = new List<Type>(method.GetParameters().Select(_ => _.ParameterType));
            fastCall = method.Bind();
        }

        public Object Invoke(Object target, Object[] parameters)
        {
            return fastCall(target, parameters);
        }
    }

    internal class ApiInfo
    {
        public List<ApiMethodInfo> Methods { get; set; }

        internal ApiInfo(Type apiClass)
        {
            Methods = new List<ApiMethodInfo>(
                apiClass.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy).Where(_ =>
                    _.GetCustomAttribute<JSRPCNet.ApiMethodAttribute>(false) != null &&
                    _.Name != "InvokeApi"
                ).Select((_, i) => new ApiMethodInfo(_, i)));
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ApiMethodAttribute : Attribute
    {
    }

    [Serializable]
    public class DeserializationException : Exception
    {
        public DeserializationException() { }
        public DeserializationException(string message) : base(message) { }
        public DeserializationException(string message, Exception inner) : base(message, inner) { }
        protected DeserializationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
    public class ApiController : System.Web.Http.ApiController
    {
        static Dictionary<Type, ApiInfo> registeredAPIs = new Dictionary<Type, ApiInfo>();
        static JavaScriptSerializer serializer = new JavaScriptSerializer();
        readonly ApiInfo thisApi;

        [HttpGet]
        public Object GetApi()
        {
            return thisApi;
        }

        [HttpPost]
        public Object InvokeApi(JObject jsonData)
        {
            dynamic json = jsonData;
            int methodIndex = json.MethodIndex;
            ApiMethodInfo methodInfo = thisApi.Methods[methodIndex];

            JToken parameterValuesToken = json.ParameterValues;
            Object[] parameterValues = new Object[methodInfo.ParameterTypes.Count];
            for (int paramIndex = 0; paramIndex < methodInfo.ParameterTypes.Count; paramIndex++)
            {
                if (methodInfo.ParameterTypes[paramIndex].IsPrimitive)
                {
                    try
                    {
                        parameterValues[paramIndex] = Convert.ChangeType(parameterValuesToken[paramIndex].ToString(), methodInfo.ParameterTypes[paramIndex]);
                    }
                    catch (System.FormatException)
                    {
                        throw new DeserializationException("Parameter " + paramIndex.ToString() + " could not be converted to type: \"" + methodInfo.ParameterTypes[paramIndex].ToString() + "\" from the JSON string \"" + parameterValuesToken[paramIndex].ToString());
                    }
                }
                else if (methodInfo.ParameterTypes[paramIndex] == typeof(string))
                    parameterValues[paramIndex] = parameterValuesToken[paramIndex].ToString();
                else
                    parameterValues[paramIndex] = serializer.Deserialize(parameterValuesToken[paramIndex].ToString(), methodInfo.ParameterTypes[paramIndex]);
            }
            return methodInfo.Invoke(this, parameterValues);
        }

        public ApiController()
        {
            RegisterThisAPI();
            thisApi = registeredAPIs[this.GetType()];
        }

        private void RegisterThisAPI()
        {
            if (!registeredAPIs.ContainsKey(this.GetType()))
                registeredAPIs[this.GetType()] = new ApiInfo(this.GetType());
        }
    }
}