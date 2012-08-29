using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Alterity.SlimAPI
{
    public class ApiInfo
    {
        public Type ApiClass { get; set; }
        public List<ApiMethodInfo> Methods { get; set; }
        
        internal ApiInfo (Type apiClass)
        {
            ApiClass = apiClass;
            Methods = new List<ApiMethodInfo>(
                apiClass.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(_ =>
                    (_.GetCustomAttribute<System.Web.Http.HttpGetAttribute>(false) != null ||
                    _.GetCustomAttribute<System.Web.Http.HttpPostAttribute>(false) != null) &&
                    _.Name != "GetAPI"
                ).Select(_ => new ApiMethodInfo(_)));
        }
    }
}