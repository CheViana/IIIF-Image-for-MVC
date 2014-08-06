using System;
using System.Drawing;

namespace IIIFImageMVC.Processing
{
    public class ScaleProcessor
    {
        public Image PercentageScaling(Bitmap image, int n)
        {
            var width = (int) Math.Floor(image.Width*(n/100.0));
            var height = (int) Math.Floor(image.Height*(n/100.0));
            return image.GetThumbnailImage(width, height, ThumbnailCallback, IntPtr.Zero);
        }

        public int ComputeHeight(int destWidth, int width, int height)
        {
            double realRatio = height/((double) width);
            var heightAfterRatio = (int) Math.Floor(realRatio*width);
            return heightAfterRatio;
        }

        public Image SizeScaling(Bitmap image, int width, int height, bool distorted)
        {
            try
            {
                double realRatio = image.Height/((double) image.Width);
                if (width != 0 & height != 0)
                {
                    if (distorted) return image.GetThumbnailImage(width, height, ThumbnailCallback, IntPtr.Zero);
                    var heightAfterRatio = (int) Math.Floor(realRatio*width);
                    var widthAfterRatio = (int) Math.Floor(height/realRatio);
                    return image.GetThumbnailImage(widthAfterRatio, heightAfterRatio, ThumbnailCallback, IntPtr.Zero);
                }
                if (width != 0)
                {
                    var heightAfterRatio = (int) Math.Floor(realRatio*width);
                    return image.GetThumbnailImage(width, heightAfterRatio, ThumbnailCallback, IntPtr.Zero);
                }
                if (height != 0)
                {
                    var widthAfterRatio = (int) Math.Floor(height/realRatio);
                    return image.GetThumbnailImage(widthAfterRatio, height, ThumbnailCallback, IntPtr.Zero);
                }
                return image;
            }
            catch (Exception)
            {
                return image;
            }
        }

        public static bool ThumbnailCallback()
        {
            return false;
        }
    }
}