using System.Collections.Generic;

namespace IIIFImageMVC.Business.Processing
{
    public class FormatConvertor
    {
        private readonly Dictionary<string, string> formatMime = new Dictionary<string, string>()
        {
            { "tif", "image/tiff"}, 
            { "png", "image/png" },
            { "gif", "image/gif" },
            { "jp2", "image/jp2" }, 
            { "jpeg", defaultMime }
        };
        private const string defaultMime = "image/jpeg";
        public string ConvertFormatToMime(string colorformat)
        {
            var parts = colorformat.Split(new []{'.'});
            var format = parts[1];
            return formatMime.ContainsKey(format) ? formatMime[format] : defaultMime;
        }
    }
}