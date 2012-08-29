using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Web.Http;

namespace Alterity.SlimAPI
{
    public class ApiMethodInfo
    {
        public string MethodName { get; set; }
        public int MethodIndex { get; set; }
        public bool UseGet { get; set; }
        public List<string> ParameterNames { get; set; }

        internal Func<Object, Object[], Object> fastCall;

        public ApiMethodInfo(MethodInfo method, int methodIndex)
        {
            MethodName = method.Name;
            MethodIndex = methodIndex;
            UseGet = method.GetCustomAttribute<HttpPostAttribute>(false) == null; //favors post if both are accepted
            ParameterNames = new List<string>(method.GetParameters().Select(_ => _.Name));
            fastCall = method.Bind();
        }

        public Object Invoke(Object target, Object[] parameters)
        {
            return fastCall(target, parameters);
        }
    }
}