using System.ComponentModel.DataAnnotations;

namespace IIIFImageMVC.Models
{
    public class ImageTileInfo
    {
        [Key]
        public int TileID { get; set; }
        public string ImageID { get; set; }
        public int Height;
        public bool IsFull;
        public bool IsScaled;
        public int ScaledWidth;
        public int Width;
        public int XOffset;
        public int YOffset;
    }
}