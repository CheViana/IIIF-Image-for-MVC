using System.Configuration;
using System.Drawing;

namespace IIIFImageMVC
{
    public class ImageProvider
    {
        public Image GetImage(string filename)
        {
            string imagesPath = ConfigurationManager.AppSettings["imagesFolderPath"];
            return Image.FromFile(imagesPath + filename);
        }
    }
}