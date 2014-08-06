using System;
using System.Drawing;

namespace IIIFImageMVC.Processing
{
    public class CropProcessor
    {
        public Bitmap SizeCrop(Image image, int x, int y, int w, int h)
        {
            return TileFromSizes(image, h, y, w, x);
        }

        public Bitmap PercentageCrop(Image image, int px, int py, int pw, int ph)
        {
            int finalWidth = DimentionFromPercToPx(image.Width, pw);
            int finalHeigth = DimentionFromPercToPx(image.Height, ph);
            int xOffset = OffsetFromPercToPx(image.Width, px);
            int yOffset = OffsetFromPercToPx(image.Height, py);
            return TileFromSizes(image, finalHeigth, yOffset, finalWidth, xOffset);
        }

        public static int OffsetFromPercToPx(int dimention, int perc)
        {
            return (int) Math.Floor(dimention*(perc/100.0));
        }

        public static int DimentionFromPercToPx(int dimention, int perc)
        {
            return (int) Math.Floor((perc/100.0)*dimention);
        }

        private static Bitmap TileFromSizes(Image image, int targetHeigth, int yOffset, int targetWidth, int xOffset)
        {
            if (xOffset == 0 && yOffset == 0 && image.Height == targetHeigth && image.Width == targetWidth)
                return (Bitmap) image;

            if (image.Height < targetHeigth + yOffset) targetHeigth = image.Height - yOffset;
            if (image.Width < targetWidth + xOffset) targetWidth = image.Width - xOffset;

            var currentTile = new Bitmap(targetWidth, targetHeigth);
            currentTile.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            Graphics currentTileGraphics = Graphics.FromImage(currentTile);
            currentTileGraphics.Clear(Color.Black);
            var absentRectangleArea = new Rectangle(xOffset, yOffset, targetWidth, targetHeigth);
            currentTileGraphics.DrawImage(image, 0, 0, absentRectangleArea, GraphicsUnit.Pixel);
            return currentTile;
        }
    }
}