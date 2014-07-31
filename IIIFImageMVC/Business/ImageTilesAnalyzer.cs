using System;
using System.Collections.Generic;
using System.Linq;
using IIIFImageMVC.Business.Processing;

namespace IIIFImageMVC.Business
{
    public class ImageTilesAnalyzer
    {
        //filling in x-yoffset,width-height only
        public IEnumerable<PageTileInfo> AnalyzeImageForTileCreation(int width, int height)
        {
            var tiles = new List<PageTileInfo>
            {
                //full tile
                new PageTileInfo() {IsFull = true, Height = height, Width = width, XOffset = 0, YOffset = 0}
            };
            AnalyzeRec(tiles, width, height, 0, 0);
            return tiles;
        }

        public IEnumerable<PageTileInfo> GenerateTilesInfo(int width, int height)
        {
            var scaleProc = new ScaleProcessor();

            yield return //full tile
                new PageTileInfo() { IsFull = true, Height = height, Width = width, XOffset = 0, YOffset = 0 };

            const int minPiece = 256;

            //counting widthMain - it's < width, but minPiece can fit there int amount of times
            int widthMain = (width / minPiece) * minPiece;
            int heightMain = (height / minPiece) * minPiece;

            //starting from 0,0 creating squares 256x256, 512x512, 1024x1024, etc
            for (int x = 0; x < widthMain; x += minPiece)
            {
                for (int y = 0; y < heightMain; y += minPiece)
                {
                    yield return new PageTileInfo() { XOffset = x, YOffset = y, Width = minPiece, Height = minPiece };

                    if (x % (minPiece * 2) == 0 && y % (minPiece * 2) == 0)
                    {
                        yield return
                            new PageTileInfo() { XOffset = x, YOffset = y, Width = minPiece * 2, Height = minPiece * 2 };

                        //adding scaled version too

                        yield return
                            new PageTileInfo()
                            {
                                XOffset = x,
                                YOffset = y,
                                Width = minPiece * 2,
                                Height = minPiece * 2,
                                IsScaled = true,
                                DestWidth = minPiece
                            };
                    }
                    else continue;

                    if (x % (minPiece * 4) == 0 && y % (minPiece * 4) == 0)
                    {
                        yield return
                            new PageTileInfo() { XOffset = x, YOffset = y, Width = minPiece * 4, Height = minPiece * 4 };
                        //adding scaled version too - 512
                        yield return
                            new PageTileInfo()
                            {
                                XOffset = x,
                                YOffset = y,
                                Width = minPiece * 4,
                                Height = minPiece * 4,
                                IsScaled = true,
                                DestWidth = minPiece * 2
                            };

                        //adding scaled version too - 256
                        yield return
                            new PageTileInfo()
                            {
                                XOffset = x,
                                YOffset = y,
                                Width = minPiece * 4,
                                Height = minPiece * 4,
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
                yield return new PageTileInfo() { XOffset = widthMain, YOffset = heightMain, Width = width - widthMain, Height = height - heightMain };
            }
            //bottom pieces
            if (height != heightMain)
            {
                var diff = height - heightMain;
                for (int x = 0; x < widthMain; x += minPiece)
                {
                    yield return
                        new PageTileInfo() { XOffset = x, YOffset = heightMain, Width = minPiece, Height = diff };
                }
            }
            //right side pieces
            if (width != widthMain)
            {
                var diff = width - widthMain;
                for (int y = 0; y < heightMain; y += minPiece)
                {
                    yield return
                        new PageTileInfo() { XOffset = widthMain, YOffset = y, Width = diff, Height = minPiece };
                }
            }
        }

        private void AnalyzeRec(ICollection<PageTileInfo> pageTiles, int width, int height, int x, int y)
        {
            const int minPiece = 256;
            if (width > minPiece & height > minPiece)
            {
                var width1 = width / 2;
                var width2 = width - width1;
                var height1 = height / 2;
                var height2 = height - height1;
                pageTiles.Add(new PageTileInfo() { IsFull = false, Width = width1, Height = height1, XOffset = x, YOffset = y });
                pageTiles.Add(new PageTileInfo() { IsFull = false, Width = width2, Height = height1, XOffset = x + width1, YOffset = y });
                pageTiles.Add(new PageTileInfo() { IsFull = false, Width = width1, Height = height2, XOffset = x, YOffset = y + height1 });
                pageTiles.Add(new PageTileInfo() { IsFull = false, Width = width2, Height = height2, XOffset = x + width1, YOffset = y + height1 });
                AnalyzeRec(pageTiles, width1, height1, x, y);
                AnalyzeRec(pageTiles, width2, height1, x + width1, y);
                AnalyzeRec(pageTiles, width1, height2, x, y + height1);
                AnalyzeRec(pageTiles, width2, height2, x + width1, y + height1);
            }
        }

        public Tuple<PageTileInfo, string> LookForClosestTile(IEnumerable<PageTileInfo> allTiles, string region)
        {
            if (region == "full") return new Tuple<PageTileInfo, string>(allTiles.First(t => t.IsFull), region);

            var tiles = allTiles.ToArray();
            if (tiles.Count() == 1) return new Tuple<PageTileInfo, string>(tiles.ElementAt(0), region);

            var fullTile = tiles.First(t => t.IsFull);

            //looking for tiles that our region fits in
            var fittedTiles = tiles.Where(pageTile => RegionFitsInTile(pageTile, region, fullTile.Width, fullTile.Height)).ToList();

            //and then looking for smallest of them
            var smallestDims = fittedTiles.Min(t => t.Width * t.Height);
            var smallestTile = fittedTiles.First(t => t.Width * t.Height == smallestDims);
            var newRegion = UpdateRegionRequestParam(smallestTile, region, fullTile.Width, fullTile.Height);
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

        private static void GetDimsInPx(string region, int width, int height, out int x, out int y, out int w, out int h)
        {
            if (region == "full")
            {
                x = 0;
                y = 0;
                w = width;
                h = height;
                return;
            }
            var regionParts = region.Split(new[] { ',', ':' });
            if (region.StartsWith("pct:"))
            {
                var xperc = int.Parse(regionParts[1]);
                var yperc = int.Parse(regionParts[2]);
                var wperc = int.Parse(regionParts[3]);
                var hperc = int.Parse(regionParts[4]);
                x = CropProcessor.OffsetFromPercToPx(width, xperc);
                y = CropProcessor.OffsetFromPercToPx(height, yperc);
                w = CropProcessor.DimentionFromPercToPx(width, wperc);
                h = CropProcessor.DimentionFromPercToPx(height, hperc);
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

        public Tuple<PageTileInfo, string, string> LookForClosestTileScaled(IEnumerable<PageTileInfo> tiles, string region, int scalingWidth)
        {
            var allTIles = tiles.ToArray();
            var fullTile = allTIles.First(t => t.IsFull);

            //looking for tiles that our region fits in
            var fittedTiles = allTIles.Where(pageTile => RegionFitsInTile(pageTile, region, fullTile.Width, fullTile.Height)).ToList();
            int x, y, w, h;
            GetDimsInPx(region, fullTile.Width, fullTile.Height, out x, out y, out w, out h);
            var fittedTile =
                fittedTiles.FirstOrDefault(
                    ft =>
                        ft.IsScaled & ft.DestWidth == scalingWidth && ft.XOffset == x && ft.YOffset == y &&
                        ft.Width == w && ft.Height == h);

            if (fittedTile.Width != 0)
            {
                return new Tuple<PageTileInfo, string, string>(fittedTile,
                    UpdateRegionRequestParam(fittedTile, region, fittedTile.Width, fittedTile.Height), "0,");
            }
            var tuple = LookForClosestTile(allTIles, region);
            return new Tuple<PageTileInfo, string, string>(tuple.Item1, tuple.Item2, scalingWidth + ",");
        }

    }
}