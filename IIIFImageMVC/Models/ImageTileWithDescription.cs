using System.ComponentModel.DataAnnotations;

namespace IIIFImageMVC.Models
{
    public class ImageTile
    {
        [Key]
        public int ID { get; set; }
        public string ImageID { get; set; }
        public byte[] Image { get; set; }
    }
}