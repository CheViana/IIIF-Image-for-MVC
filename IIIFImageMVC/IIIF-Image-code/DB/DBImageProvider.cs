using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IIIFImageMVC.Models;
using IIIFImageMVC.Processing;

namespace IIIFImageMVC.DB
{
    public class DbImageProvider : IImageProviderWithCaching
    {
        private const string imagesFolderPath = "imagesFolderPath";
        
        private readonly ImageTilesAnalyzer analyzer;
        private readonly CropProcessor cropProcessor;
        private readonly ScaleProcessor scaleProcessor;
        public DbImageProvider(ImageTilesAnalyzer imageTilesAnalyzer, CropProcessor cropProc, ScaleProcessor scaleProc)
        {
            analyzer = imageTilesAnalyzer;
            cropProcessor = cropProc;
            scaleProcessor = scaleProc;
        }

        public ImageTileInfo GetFullImageInfo(string id)
        {
            using (var context = new ImageTilesDbContext())
            {
                var info = context.ImageTileInfos.FirstOrDefault(im => im.ImageID == id && im.IsFull);
                if (info != null)
                {
                    return info;
                }
            }
            var task = new Task(() => AddImageToDb(id));
            task.Start();
            var image = GetImageFromFileSystem(id);
            return new ImageTileInfo()
            {
                Height = image.Height,
                Width = image.Width,
                ImageID = id,
                IsFull = true,
                IsScaled = false,
                XOffset = 0,
                YOffset = 0
            };
        }

        public Tuple<Image, string, string> GetImageTileCached(string id, string region, string size)
        {
            using (var context = new ImageTilesDbContext())
            {
                var allTilesForThisImage = context.ImageTileInfos.Where(im => im.ImageID == id);
                if (allTilesForThisImage.Any())
                {
                    var complicatedTileInfo = analyzer.LookForClosestTileScaled(allTilesForThisImage, region,
                        int.Parse(size.Split(',')[0]));
                    var imageTile = context.ImageTiles.FirstOrDefault(im => im.ID == complicatedTileInfo.Item1.TileID);
                    using (var memStream = new MemoryStream())
                    {
                        memStream.Write(imageTile.Image, 0, imageTile.Image.Length);
                        return new Tuple<Image, string, string>(Image.FromStream(memStream), complicatedTileInfo.Item2, complicatedTileInfo.Item3);
                    }
                }
            }
            var task = new Task(() => AddImageToDb(id));
            task.Start();
            return new Tuple<Image, string, string>(GetImageFromFileSystem(id), region, size);
        }

        private static Image GetImageFromFileSystem(string id)
        {
            string imagesPath = ConfigurationManager.AppSettings[imagesFolderPath];
            var files = Directory.GetFiles(imagesPath, id + "*");
            return Image.FromFile(files.FirstOrDefault());
        }

        private void AddImageToDb(string id)
        {
            using (var image = GetImageFromFileSystem(id))
            {
                using (var context = new ImageTilesDbContext())
                {

                    context.ImageTileInfos.RemoveRange(context.ImageTileInfos.Where(im => im.ImageID == id));
                    context.ImageTiles.RemoveRange(context.ImageTiles.Where(im => im.ImageID == id));
                    context.SaveChanges();

                    var tiles = analyzer.GenerateTilesInfo(image.Width, image.Height);
                    foreach (var imageTileInfo in tiles)
                    {
                        imageTileInfo.ImageID = id;
                        context.ImageTileInfos.Add(imageTileInfo);
                    }
                    context.SaveChanges();
                    foreach (var imageTileInfo in context.ImageTileInfos.Where(iti=>iti.ImageID == id))
                    {
                        Image tileImage = cropProcessor.SizeCrop(image, imageTileInfo.XOffset, imageTileInfo.YOffset, imageTileInfo.Width, imageTileInfo.Height);
                        if (imageTileInfo.IsScaled)
                        {
                            var scaled = scaleProcessor.SizeScaling((Bitmap)tileImage, imageTileInfo.ScaledWidth, 0, false);
                            tileImage = scaled;
                        }
                        using (var memStream = new MemoryStream())
                        {
                            tileImage.Save(memStream, ImageFormat.Jpeg);
                            var bytes = memStream.ToArray();
                            var ptc = new ImageTile() { ImageID = id, ID = imageTileInfo.TileID, Image = bytes };
                            context.ImageTiles.Add(ptc);
                        }
                    }
                    context.SaveChanges();
                }

            }
        }
    }
}