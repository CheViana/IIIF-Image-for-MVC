namespace IIIFImageMVC.Business.Processing
{
    public class FormatConvertor
    {
        public string ConvertFormatToMime(string colorformat)
        {
            var parts = colorformat.Split(new []{'.'});
            var format = parts[1];
            /*
                 * jpg	image/jpeg
                    tif	image/tiff
                    png	image/png
                    gif	image/gif
                    jp2	image/jp2
                    pdf	application/pdf
                 */
            var mime = string.Empty;
            switch (format)
            {
                case "tif":
                    mime = "image/tiff";
                    break;
                case "png":
                    mime = "image/png";
                    break;
                case "gif":
                    mime = "image/gif";
                    break;
                case "jp2":
                    mime = "image/jp2";
                    break;
                default:
                    mime = "image/jpeg";
                    break;
            }
            return mime;
        }

    }
}