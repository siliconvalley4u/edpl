using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EnterpriseDataPipeline
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "FramedApp",
            //    url: "{controller}/{action}/{*page}",
            //    defaults: new { controller = "CubeController", action = "ViewPage", page = "" }
            //);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );



            //routes.MapRoute(
            //    "FramedApp",
            //    "{controller}/{action}/{*page}",
            //     new { controller = "DashBoardController", action = "ViewPage", page = "" }); 
        }
    }
}
