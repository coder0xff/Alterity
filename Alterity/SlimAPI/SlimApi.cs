using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;

namespace Alterity.SlimAPI
{
    public class SlimApi : ApiController
    {
        static Dictionary<Type, ApiInfo> registeredAPIs = new Dictionary<Type, ApiInfo>();
        static JavaScriptSerializer serializer = new JavaScriptSerializer();
        readonly ApiInfo thisApi;

        [HttpGet]
        public ApiInfo GetApi()
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
                parameterValues[paramIndex] = serializer.Deserialize(parameterValuesToken[paramIndex.ToString()].ToString(), methodInfo.ParameterTypes[paramIndex]);
            return methodInfo.Invoke(this, parameterValues);
        }

        public SlimApi()
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
