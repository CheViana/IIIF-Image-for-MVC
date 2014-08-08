using System;
using System.Drawing;
using IIIFImageMVC.Models;

namespace IIIFImageMVC.DB
{
    public interface IImageProviderWithCaching
    {
        Tuple<Image, string, string> GetImageTileCached(string id, string region, string size);
        ImageTileInfo GetFullImageInfo(string id);
    }
}