using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace IIIFImageMVC
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //id1/full/full/0/native
            //id1/0,10,100,200/pct:50/90/native.png
            routes.MapRoute(
                "ImageTile",
                "images/{id}/{region}/{size}/{rotation}/{colorformat}",
                new { controller = "Image", action = "GetImageTile" }
            );

            //id1/info.json
            routes.MapRoute(
                "Info",
                "images/{id}/info.json",
                new { controller = "Image", action = "Info" }
            );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}