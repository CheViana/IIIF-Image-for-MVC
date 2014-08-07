using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using IIIFImageMVC.Processing;
using System.Collections.Generic;

namespace IIIFImageMVC.Controllers
{
    public class ImageController : Controller
    {
        private Dictionary<string, object> infoTemplate = new Dictionary<string, object>()
                        { 
                            { "@context", "http://library.stanford.edu/iiif/image-api/1.1/context.json" },
                            { "@id", "" },
                            { "width", 0 },
                            { "height", 0 },
                            { "scale_factors", new[] { 1, 2, 4, 8, 16, 32, 64 } },
                            { "tile_width", 256 },
                            { "tile_height", 256 },
                            { "formats", new[] { "jpg","png" } },
                            { "qualities", new[] { "native","greyscale" }},
                            { "profile", "http://library.stanford.edu/iiif/image-api/1.1/compliance.html#level2"}
                        };
        private MainProcessor mainProc = new MainProcessor();        
        private ImageProvider imageProvider = new ImageProvider();  
        private const string rootUrlForImages = "/images/";
        private const string defaultColorFormat = "native.jpg";
        private const int defaultRotation = 0;

        public ImageController()
        {            
        }

        public ActionResult GetImageTile(string id, string region, string size, float rotation = defaultRotation, string colorformat = defaultColorFormat)
        {
            var imageFull = imageProvider.GetImage(id);
            Bitmap imageReady = mainProc.GetImageTile(imageFull, region, size, rotation, colorformat);           
            using (var memStream = new MemoryStream())
            {                
                imageReady.Save(memStream, ImageFormat.Jpeg);                
                var mime = mainProc.FormatConvertor.ConvertFormatToMime(colorformat);
                return File( memStream.ToArray(), mime);
            }
        } 
 
        public JsonResult Info(string id)
        {
            var image = imageProvider.GetImage(id);
            var ourInfo = new Dictionary<string,object>(infoTemplate);
            ourInfo["@id"] = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + rootUrlForImages + id;
            ourInfo["width"] = image.Width;
            ourInfo["height"] = image.Height;
            return Json(ourInfo, JsonRequestBehavior.AllowGet);
        }
    }
}
