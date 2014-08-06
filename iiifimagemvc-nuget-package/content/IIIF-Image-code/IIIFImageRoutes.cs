using System.Web.Mvc;
using System.Web.Routing;

public class IIIFImageRoutes
{
		public static void RegisterRoutes(RouteCollection routes)
        {
			//id1/full/full/0/native
            //id1/0,10,100,200/pct:50/90/native.png
            routes.MapRoute(
                "ImageTile",
                "images/{id}/{region}/{size}/{rotation}/{colorformat}",
                new {controller = "Image", action = "GetImageTile"}
                );

            //id1/info.json
            routes.MapRoute(
                "Info",
                "images/{id}/info.json",
                new {controller = "Image", action = "Info"}
                );
        }
}