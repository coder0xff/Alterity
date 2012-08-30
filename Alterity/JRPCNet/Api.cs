using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;

namespace JRPCNet
{
    public class Api : ApiController
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
                    parameterValues[paramIndex] = Convert.ChangeType(parameterValuesToken[paramIndex.ToString()].ToString(), methodInfo.ParameterTypes[paramIndex]);
                else if (methodInfo.ParameterTypes[paramIndex] == typeof(string))
                    parameterValues[paramIndex] = parameterValuesToken[paramIndex.ToString()].ToString();
                else
                    parameterValues[paramIndex] = serializer.Deserialize(parameterValuesToken[paramIndex.ToString()].ToString(), methodInfo.ParameterTypes[paramIndex]);
            }
            return methodInfo.Invoke(this, parameterValues);
        }

        public Api()
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
