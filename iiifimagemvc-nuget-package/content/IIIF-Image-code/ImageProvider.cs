using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;

namespace IIIFImageMVC
{
    public class ImageProvider
    {
        public Image GetImage(string id)
        {
            string imagesPath = ConfigurationManager.AppSettings["imagesFolderPath"];
            var files = Directory.GetFiles(imagesPath,id+"*");            
            return Image.FromFile(files.FirstOrDefault());
        }
    }
}