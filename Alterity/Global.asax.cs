using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data.Entity;
using Alterity.Models;

namespace Alterity
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mvc")]
    public class MvcApplication : System.Web.HttpApplication
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        protected void Application_Start()
        {
#if DEVELOPMENT_ENVIRONMENT
            Database.SetInitializer<EntityMappingContext>(new DatabaseInitializer());
#endif
            ModelBinders.Binders.DefaultBinder = new JsonModelBinder();
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }

    public class JsonModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext,
            ModelBindingContext bindingContext)
        {
            // If the request is not of content type "application/json"
            // then use the default model binder.
            if (!IsJSONRequest(controllerContext))
            {
                return base.BindModel(controllerContext, bindingContext);
            }

            // Get the JSON data posted
            var request = controllerContext.HttpContext.Request;
            var jsonStringData =
                new System.IO.StreamReader(request.InputStream).ReadToEnd();

            return new System.Web.Script.Serialization.JavaScriptSerializer()
                .Deserialize(jsonStringData, bindingContext.ModelMetadata.ModelType);
        }

        private bool IsJSONRequest(ControllerContext controllerContext)
        {
            var contentType = controllerContext.HttpContext.Request.ContentType;
            return contentType.Contains("application/json");
        }
    }
}