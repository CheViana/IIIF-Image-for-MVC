using System;
using System.Drawing;
using IIIFImageMVC.Processing;

namespace IIIFImageMVC
{
    public class MainProcessor
    {
        public Bitmap Crope(Image im, string region)
        {
            var cropProcessor = new CropProcessor();
            //full ?
            if (region == "full") return (Bitmap) (im);

            // percentage or coordinates cropp ?
            string[] parts = region.Split(new[] {',', ':'});
            // region=pct:10,10,80,80
            if (region.StartsWith("pct:"))
            {
                return cropProcessor.PercentageCrop(im, int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]),
                    int.Parse(parts[4]));
            }
            // region=0,10,100,200
            return cropProcessor.SizeCrop(im, int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]),
                int.Parse(parts[3]));
        }

        public Image Scale(Bitmap bitmap, string size)
        {
            var scaleProcessor = new ScaleProcessor();
            //full ?
            if (size == "full") return bitmap;

            // percentage or coordinates scale ?                     
            string[] parts = size.Split(new[] {',', ':'});
            // size=pct:50
            if (size.StartsWith("pct:"))
            {
                return scaleProcessor.PercentageScaling(bitmap, int.Parse(parts[1]));
            }
            // size=50,
            if (string.IsNullOrEmpty(parts[1]))
            {
                return scaleProcessor.SizeScaling(bitmap, int.Parse(parts[0]), 0, false);
            }
            // size=,50
            if (string.IsNullOrEmpty(parts[0]))
            {
                return scaleProcessor.SizeScaling(bitmap, 0, int.Parse(parts[1]), false);
            }
            // size=50,50
            return scaleProcessor.SizeScaling(bitmap, int.Parse(parts[0]), int.Parse(parts[1]), true);
        }

        public Bitmap ColorRotateFormat(Image im, float rotate, string colorformat)
        {
            var rotatedProcessor = new RotateProcessor();
            var colorProcessor = new ColorProcessor();
            string color = "";
            if (colorformat.Contains("."))
            {
                string[] parts = colorformat.Split('.');
                if (!string.IsNullOrWhiteSpace(parts[0])) color = parts[0];
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(colorformat)) color = colorformat;
            }

            var bitmap = new Bitmap(im);
            if (Math.Abs(rotate) > 0.0001)
            {
                Bitmap rotatedImage = rotatedProcessor.RotateImg(bitmap, rotate);
                bitmap = rotatedImage;
            }
            if (!string.IsNullOrEmpty(color))
            {
                Bitmap colorImage = colorProcessor.ChangeColor(bitmap, color);
                bitmap = colorImage;
            }
            return bitmap;
        }
    }
}