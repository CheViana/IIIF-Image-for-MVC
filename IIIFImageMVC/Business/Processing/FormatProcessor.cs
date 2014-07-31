namespace IIIFImageMVC.Business.Processing
{
    public class FormatProcessor
    {
        public string ConvertFormatToMime(string format)
        {
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