using System.Collections.Generic;

namespace IIIFImageMVC.Processing
{
    public class FormatConvertor
    {
        private const string defaultMime = "image/jpeg";

        private readonly Dictionary<string, string> formatMime = new Dictionary<string, string>
        {
            {"tif", "image/tiff"},
            {"png", "image/png"},
            {"gif", "image/gif"},
            {"jp2", "image/jp2"},
            {"jpeg", defaultMime}
        };

        public string ConvertFormatToMime(string colorformat)
        {
            string[] parts = colorformat.Split(new[] {'.'});
            string format = parts[1];
            return formatMime.ContainsKey(format) ? formatMime[format] : defaultMime;
        }
    }
}