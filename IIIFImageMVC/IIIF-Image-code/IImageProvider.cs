using System;
using System.Drawing;
namespace IIIFImageMVC.IIIF_Image_code
{
    public interface IImageProvider
    {
        Image GetImage(string id);
    }
}
