using System.Data.Entity;

namespace IIIFImageMVC.Models
{
    public class ImageTilesDbContext : DbContext
    {
        public virtual DbSet<ImageTile> ImageTiles { get; set; }
        public virtual DbSet<ImageTileInfo> ImageTileInfos { get; set; } 
    }
}