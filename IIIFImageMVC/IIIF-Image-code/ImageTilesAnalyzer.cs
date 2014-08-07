using System;
using System.Collections.Generic;
using System.Linq;
using IIIFImageMVC.Processing;

namespace IIIFImageMVC
{
    public class ImageTilesAnalyzer
    {
        private ScaleProcessor scaleProcessor;
        private CropProcessor cropProcessor;
        private const int minPiece = 256;
        public int MinPiece
        {
            get { return minPiece; }
        } 

        public ImageTilesAnalyzer(ScaleProcessor scaleProcessor, CropProcessor cropProcessor)
        {
            this.scaleProcessor = scaleProcessor;
            this.cropProcessor = cropProcessor;
        }

        #region analyzing-for-tile-creation - dividing image to 4 same parts till parts are less than minPiece. not in use

        //filling in x-yoffset,width-height only
        public IEnumerable<PageTileInfo> AnalyzeImageForTileCreation(int width, int height)
        {
            var tiles = new List<PageTileInfo>
            {
                //full tile
                new PageTileInfo {IsFull = true, Height = height, Width = width, XOffset = 0, YOffset = 0}
            };
            //recursive analysis - for each tile analyzise it's subtiles
            AnalyzeRec(tiles, width, height, 0, 0);
            return tiles;
        }

        private void AnalyzeRec(ICollection<PageTileInfo> pageTiles, int width, int height, int x, int y)
        {
            if (width > minPiece & height > minPiece)
            {
                int width1 = width / 2;
                int width2 = width - width1;
                int height1 = height / 2;
                int height2 = height - height1;
                pageTiles.Add(new PageTileInfo
                {
                    IsFull = false,
                    Width = width1,
                    Height = height1,
                    XOffset = x,
                    YOffset = y
                });
                pageTiles.Add(new PageTileInfo
                {
                    IsFull = false,
                    Width = width2,
                    Height = height1,
                    XOffset = x + width1,
                    YOffset = y
                });
                pageTiles.Add(new PageTileInfo
                {
                    IsFull = false,
                    Width = width1,
                    Height = height2,
                    XOffset = x,
                    YOffset = y + height1
                });
                pageTiles.Add(new PageTileInfo
                {
                    IsFull = false,
                    Width = width2,
                    Height = height2,
                    XOffset = x + width1,
                    YOffset = y + height1
                });
                AnalyzeRec(pageTiles, width1, height1, x, y);
                AnalyzeRec(pageTiles, width2, height1, x + width1, y);
                AnalyzeRec(pageTiles, width1, height2, x, y + height1);
                AnalyzeRec(pageTiles, width2, height2, x + width1, y + height1);
            }
        }
        #endregion

        public IEnumerable<PageTileInfo> GenerateTilesInfo(int width, int height)
        {
            yield return //full tile
                new PageTileInfo {IsFull = true, Height = height, Width = width, XOffset = 0, YOffset = 0};            

            //counting widthMain - it's < width, but minPiece can fit there int amount of times
            int widthMain = (width/minPiece)*minPiece;
            int heightMain = (height/minPiece)*minPiece;

            //starting from 0,0 creating squares 256x256, 512x512, 1024x1024, etc
            for (int x = 0; x < widthMain; x += minPiece)
            {
                for (int y = 0; y < heightMain; y += minPiece)
                {
                    //256x256
                    yield return new PageTileInfo {XOffset = x, YOffset = y, Width = minPiece, Height = minPiece};

                    if (x%(minPiece*2) == 0 && y%(minPiece*2) == 0)
                    {
                        //512x512
                        yield return
                            new PageTileInfo {XOffset = x, YOffset = y, Width = minPiece*2, Height = minPiece*2};

                        //512x512 scaled to 256x256
                        yield return
                            new PageTileInfo
                            {
                                XOffset = x,
                                YOffset = y,
                                Width = minPiece*2,
                                Height = minPiece*2,
                                IsScaled = true,
                                DestWidth = minPiece
                            };
                    }
                    else continue;
                    
                    if (x%(minPiece*4) == 0 && y%(minPiece*4) == 0)
                    {
                        //1024x1024
                        yield return
                            new PageTileInfo {XOffset = x, YOffset = y, Width = minPiece*4, Height = minPiece*4};
                        //1024x1024 scaled to - 512x512
                        yield return
                            new PageTileInfo
                            {
                                XOffset = x,
                                YOffset = y,
                                Width = minPiece*4,
                                Height = minPiece*4,
                                IsScaled = true,
                                DestWidth = minPiece*2
                            };

                        //1024x1024 scaled to - 256x256
                        yield return
                            new PageTileInfo
                            {
                                XOffset = x,
                                YOffset = y,
                                Width = minPiece*4,
                                Height = minPiece*4,
                                IsScaled = true,
                                DestWidth = minPiece
                            };
                    }
                }
            }

            //dealing with rest border pieces in image
            //corner piece
            if (heightMain == height && widthMain == width) yield break;
            if (heightMain != height && widthMain != width)
            {
                yield return
                    new PageTileInfo
                    {
                        XOffset = widthMain,
                        YOffset = heightMain,
                        Width = width - widthMain,
                        Height = height - heightMain
                    };
            }
            //bottom pieces
            if (height != heightMain)
            {
                int diff = height - heightMain;
                for (int x = 0; x < widthMain; x += minPiece)
                {
                    yield return
                        new PageTileInfo {XOffset = x, YOffset = heightMain, Width = minPiece, Height = diff};
                }
            }
            //right side pieces
            if (width != widthMain)
            {
                int diff = width - widthMain;
                for (int y = 0; y < heightMain; y += minPiece)
                {
                    yield return
                        new PageTileInfo {XOffset = widthMain, YOffset = y, Width = diff, Height = minPiece};
                }
            }
        }        

        public Tuple<PageTileInfo, string> LookForClosestTile(IEnumerable<PageTileInfo> allTiles, string region)
        {
            if (region == "full") return new Tuple<PageTileInfo, string>(allTiles.First(t => t.IsFull), region);

            PageTileInfo[] tiles = allTiles.ToArray();
            if (tiles.Count() == 1) return new Tuple<PageTileInfo, string>(tiles.ElementAt(0), region);

            PageTileInfo fullTile = tiles.First(t => t.IsFull);

            //looking for tiles that our region fits in
            List<PageTileInfo> fittedTiles =
                tiles.Where(pageTile => RegionFitsInTile(pageTile, region, fullTile.Width, fullTile.Height)).ToList();

            //and then looking for smallest of them
            int smallestDims = fittedTiles.Min(t => t.Width*t.Height);
            PageTileInfo smallestTile = fittedTiles.First(t => t.Width*t.Height == smallestDims);
            string newRegion = UpdateRegionRequestParam(smallestTile, region, fullTile.Width, fullTile.Height);
            return new Tuple<PageTileInfo, string>(smallestTile, newRegion);
        }

        private bool RegionFitsInTile(PageTileInfo pageTile, string region, int width, int height)
        {
            int x, y, h, w;
            GetDimsInPx(region, width, height, out x, out y, out w, out h);
            return (x >= pageTile.XOffset && x < pageTile.XOffset + pageTile.Width &&
                    y >= pageTile.YOffset && y < pageTile.YOffset + pageTile.Height &&
                    x + w <= pageTile.XOffset + pageTile.Width &&
                    y + h <= pageTile.YOffset + pageTile.Height);
        }

        private void GetDimsInPx(string region, int width, int height, out int x, out int y, out int w, out int h)
        {
            if (region == "full")
            {
                x = 0;
                y = 0;
                w = width;
                h = height;
                return;
            }
            string[] regionParts = region.Split(new[] {',', ':'});
            if (region.StartsWith("pct:"))
            {
                int xperc = int.Parse(regionParts[1]);
                int yperc = int.Parse(regionParts[2]);
                int wperc = int.Parse(regionParts[3]);
                int hperc = int.Parse(regionParts[4]);
                x = cropProcessor.OffsetFromPercToPx(width, xperc);
                y = cropProcessor.OffsetFromPercToPx(height, yperc);
                w = cropProcessor.DimentionFromPercToPx(width, wperc);
                h = cropProcessor.DimentionFromPercToPx(height, hperc);
            }
            else
            {
                x = int.Parse(regionParts[0]);
                y = int.Parse(regionParts[1]);
                w = int.Parse(regionParts[2]);
                h = int.Parse(regionParts[3]);
            }
        }

        private string UpdateRegionRequestParam(PageTileInfo tile, string oldRegion, int width, int height)
        {
            if (oldRegion == "full") return "full";
            int x, y, h, w;
            GetDimsInPx(oldRegion, width, height, out x, out y, out w, out h);

            //if we have exact region in tile
            if (tile.Height == h && tile.Width == w && tile.XOffset == x && tile.YOffset == y)
            {
                return "full";
            }

            //old w and h remains, but x and y are going to get new values
            return (x - tile.XOffset) + "," + (y - tile.YOffset) + "," + w + "," + h;
        }

        public Tuple<PageTileInfo, string, string> LookForClosestTileScaled(IEnumerable<PageTileInfo> tiles,
            string region, int scalingWidth)
        {
            PageTileInfo[] allTIles = tiles.ToArray();
            PageTileInfo fullTile = allTIles.First(t => t.IsFull);

            //looking for tiles that our region fits in
            List<PageTileInfo> fittedTiles =
                allTIles.Where(pageTile => RegionFitsInTile(pageTile, region, fullTile.Width, fullTile.Height)).ToList();
            int x, y, w, h;
            GetDimsInPx(region, fullTile.Width, fullTile.Height, out x, out y, out w, out h);
            PageTileInfo fittedTile =
                fittedTiles.FirstOrDefault(
                    ft =>
                        ft.IsScaled & ft.DestWidth == scalingWidth && ft.XOffset == x && ft.YOffset == y &&
                        ft.Width == w && ft.Height == h);

            if (fittedTile.Width != 0)
            {
                return new Tuple<PageTileInfo, string, string>(fittedTile,
                    UpdateRegionRequestParam(fittedTile, region, fittedTile.Width, fittedTile.Height), "0,");
            }
            Tuple<PageTileInfo, string> tuple = LookForClosestTile(allTIles, region);
            return new Tuple<PageTileInfo, string, string>(tuple.Item1, tuple.Item2, scalingWidth + ",");
        }
    }
}