using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Web.Http;

namespace JRPCNet
{
    internal class ApiMethodInfo
    {
        public string MethodName { get; set; }
        public int MethodIndex { get; set; }
        public List<string> ParameterNames { get; set; }

        internal List<Type> ParameterTypes { get; set; }
        internal Func<Object, Object[], Object> fastCall;

        public ApiMethodInfo(MethodInfo method, int methodIndex)
        {
            MethodName = method.Name;
            MethodIndex = methodIndex;
            ParameterNames = new List<string>(method.GetParameters().Select(_ => _.Name));
            ParameterTypes = new List<Type>(method.GetParameters().Select(_ => _.ParameterType));
            fastCall = method.Bind();
        }

        public Object Invoke(Object target, Object[] parameters)
        {
            return fastCall(target, parameters);
        }
    }
}