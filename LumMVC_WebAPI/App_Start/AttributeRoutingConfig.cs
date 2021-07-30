using System.Web.Routing;
using AttributeRouting.Web.Mvc;
using SystemLibrary.Utility;

[assembly: WebActivator.PreApplicationStartMethod(typeof(LumMVC_WebAPI.AttributeRoutingConfig), "Start")]

namespace LumMVC_WebAPI 
{
    public static class AttributeRoutingConfig
	{
		public static void RegisterRoutes(RouteCollection routes) 
		{    
			// See http://github.com/mccalltd/AttributeRouting/wiki for more options.
			// To debug routes locally using the built in ASP.NET development server, go to /routes.axd
            
			routes.MapAttributeRoutes();
		}

        public static void Start() 
		{
            RegisterRoutes(RouteTable.Routes);
        }
    }
}
