using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using IIIFImageMVC.Business;
using IIIFImageMVC.Business.Processing;

namespace IIIFImageMVC.Controllers
{
    public class ImageController : Controller
    {

        public ActionResult GetImageTile(string id, string region, string size, float rotation = 0, string colorformat = "native.jpg")
        {
            var imageFull = new ImageProvider().GetImage(id + ".jpg");
            var formatConvertor = new FormatConvertor();
            var mainProc = new MainProcessor();
            Image imageReady;
            var croppedImage = mainProc.Crope(imageFull, region);
            var scaledImage = mainProc.Scale(croppedImage, size);
            if (Math.Abs(rotation) < 0.1 && colorformat == "native.jpg")
            {
                imageReady = scaledImage;
            }
            else
            {
                imageReady = mainProc.ColorRotateFormat(scaledImage, rotation, colorformat); ;
            }
            using (var memStream = new MemoryStream())
            {
                imageReady.Save(memStream, ImageFormat.Jpeg);
                var bytes = memStream.ToArray();
                var mime = formatConvertor.ConvertFormatToMime(colorformat);
                return File(bytes, mime);
            }
        }

        public JsonResult Info(string id, string width, string height)
        {
            var uri = HttpContext.Request.Url;
            var host = uri != null ? uri.GetLeftPart(UriPartial.Authority) : "rarebooks.univ.kiev.ua";
            return Json(
                new
                {
                    @context = "http://library.stanford.edu/iiif/image-api/1.1/context.json",
                    @id = host + "/images/" + id,
                    width = width,
                    height = height,
                    scale_factors = new[] { 1, 2, 4, 8, 16, 32, 64 },
                    tile_width = 512,
                    tile_height = 512,
                    formats = new[] { "jpg" },
                    qualities = new[] { "native" },
                    profile = "http://library.stanford.edu/iiif/image-api/1.1/compliance.html#level2"
                }, JsonRequestBehavior.AllowGet);
        }


    }
}
