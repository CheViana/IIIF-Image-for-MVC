using System.Configuration;
using System.Drawing;

namespace IIIFImageMVC.Business
{
    public class ImageProvider
    {
        public Image GetImage(string filename)
        {
            var imagesPath = ConfigurationManager.AppSettings["imagesFolderPath"];
            return Image.FromFile(imagesPath + filename);
        }
    }
}