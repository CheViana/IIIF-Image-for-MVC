using System;
using System.Drawing;
using IIIFImageMVC.Processing;

namespace IIIFImageMVC
{
    public class MainProcessor
    {
        private FormatConvertor formatConvertor;
        private RotateProcessor rotatedProcessor;
        private ColorProcessor colorProcessor;
        private CropProcessor cropProcessor;
        private ScaleProcessor scaleProcessor;

        private const string defaultColorFormat = "native.jpg";
        private const string fullStr = "full";
        private const string percentageStr = "pct:";

        public MainProcessor(FormatConvertor formatConv, RotateProcessor rotatedProc, ColorProcessor colorProc, CropProcessor cropProc, ScaleProcessor scaleProc)
        {
            formatConvertor = formatConv;
            rotatedProcessor = rotatedProc;
            colorProcessor = colorProc;
            cropProcessor = cropProc;
            scaleProcessor = scaleProc;
        }        
        
        public FormatConvertor FormatConvertor
        {
            get
            {
                return formatConvertor;
            }
        }
        
        public string DefaultColorFormat
        {
            get { return defaultColorFormat; }
        }

        private Bitmap Crope(Image im, string region)        
        {
            //full ?
            if (region == fullStr) return (Bitmap)(im);

            // percentage or coordinates cropp ?
            string[] parts = region.Split(new[] {',', ':'});
            // region=pct:10,10,80,80
            if (region.StartsWith(percentageStr))
            {
                return cropProcessor.PercentageCrop(im, int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]),
                    int.Parse(parts[4]));
            }
            // region=0,10,100,200
            return cropProcessor.SizeCrop(im, int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]),
                int.Parse(parts[3]));
        }

        private Image Scale(Bitmap bitmap, string size)
        {            
            //full ?
            if (size == fullStr) return bitmap;

            // percentage or coordinates scale ?                     
            string[] parts = size.Split(new[] {',', ':'});
            // size=pct:50
            if (size.StartsWith(percentageStr))
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

        private Bitmap ColorRotate(Image im, float rotate, string colorformat)
        {
            if (Math.Abs(rotate) < 0.1 && colorformat == defaultColorFormat)
            {
                return new Bitmap(im);
            }            
            string color = "";
            string[] parts = colorformat.Split('.');
            color = parts[0];           
            var bitmap = new Bitmap(im);
            if (Math.Abs(rotate) > 0.1)
            {
                bitmap = rotatedProcessor.RotateImg(bitmap, rotate);
            }
            if (!string.IsNullOrEmpty(color) && colorformat != defaultColorFormat)
            {
                bitmap = colorProcessor.ChangeColor(bitmap, color);
            }
            return bitmap;
        }

        public Image GetImageTile(Image imageFull, string region, string size, float rotation, string colorformat)
        {
            if (region == "full" && (size == "full" || size == "0,") && Math.Abs(rotation) < 0.1 && colorformat == defaultColorFormat)
            {
                return imageFull;
            }
            Bitmap croppedImage = Crope(imageFull, region);
            Image scaledImage = Scale(croppedImage, size);
            Bitmap imageReady = ColorRotate(scaledImage, rotation, colorformat);
            return imageReady;
        }        
    }
}