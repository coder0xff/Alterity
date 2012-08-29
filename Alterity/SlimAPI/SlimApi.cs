using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace Alterity.SlimAPI
{
    public class SlimApi : ApiController
    {
        static Dictionary<Type, ApiInfo> registeredAPIs = new Dictionary<Type, ApiInfo>();
        readonly ApiInfo thisApi;

        [HttpGet]
        public ApiInfo GetApi()
        {
            return thisApi;
        }
        
        [HttpPost, HttpGet]
        public Object InvokeAPI(JObject jsonData)
        {            
            dynamic json = jsonData;

            ApiMethodInfo methodInfo = thisApi.Methods[json.SlimAPIMethodIndex];
            Object[] parameters = methodInfo.ParameterNames.Select(_ => (Object)json[_]).ToArray();
            return methodInfo.Invoke(this, parameters);
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
