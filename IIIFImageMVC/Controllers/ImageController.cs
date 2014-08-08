using System;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using IIIFImageMVC.DB;
using System.Collections.Generic;

namespace IIIFImageMVC.Controllers
{
    public class ImageController : Controller
    {
        private readonly Dictionary<string, object> infoTemplate = new Dictionary<string, object>()
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
        private readonly MainProcessor mainProcessor;
        private readonly IImageProviderWithCaching imageProvider;  
        private const string rootUrlForImages = "/images/";
        private const string defaultColorFormat = "native.jpg";
        private const int defaultRotation = 0;
        
        public ImageController(MainProcessor mainProc, IImageProviderWithCaching imageProv)
        {
            mainProcessor = mainProc;
            imageProvider = imageProv;
        }

        public FileContentResult GetImageTile(string id, string region, string size, float rotation = defaultRotation, string colorformat = defaultColorFormat)
        {
            var imageFull = imageProvider.GetImageTileCached(id, region, size);
            var imageReady = mainProcessor.GetImageTile(imageFull.Item1, imageFull.Item2, imageFull.Item3, rotation, colorformat);           
            using (var memStream = new MemoryStream())
            {                
                imageReady.Save(memStream, ImageFormat.Jpeg);                
                var mime = mainProcessor.FormatConvertor.ConvertFormatToMime(colorformat);
                return File(memStream.ToArray(), mime);
            }
        } 
 
        public JsonResult Info(string id)
        {
            var imageInfo = imageProvider.GetFullImageInfo(id);
            var ourInfo = new Dictionary<string,object>(infoTemplate);
            ourInfo["@id"] = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + rootUrlForImages + id;
            ourInfo["width"] = imageInfo.Width;
            ourInfo["height"] = imageInfo.Height;
            return Json(ourInfo, JsonRequestBehavior.AllowGet);
        }
    }
}
