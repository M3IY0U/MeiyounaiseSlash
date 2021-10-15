using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using IF.Lastfm.Core.Objects;
using MeiyounaiseSlash.Commands.Last;
using MeiyounaiseSlash.Utilities;
using SkiaSharp;

namespace MeiyounaiseSlash.Services
{
    public static class AlbumChartImageService
    {
        private const int Offset = 5;
        private const int AlbumSize = 300;

        public static Stream GenerateChart(IReadOnlyList<LastAlbum> items)
        {
            using var surface =
                SKSurface.Create(new SKImageInfo(Math.Min(items.Count * AlbumSize, 1500),
                    ((items.Count - 1) / 5 + 1) * AlbumSize));
            var canvas = surface.Canvas;
            var paint = Constants.ChartFont;

            canvas.Clear(SKColors.Transparent);

            var x = 0f;
            var y = 0f;
            var count = 0;
            var wc = new WebClient();
            foreach (var album in items)
            {
                canvas.DrawImage(
                    SKImage.FromEncodedData(
                        wc.DownloadData(album.Images.Largest ?? new Uri(Constants.LastFmUnknownAlbum))), x, y);

                var by = y;
                y = LastUtil.DrawWrappedText($"{album.ArtistName} -", ref canvas, x + Offset, y + 25,
                    AlbumSize - Offset, paint);
                LastUtil.DrawWrappedText($"{album.Name}", ref canvas, x + Offset, y, AlbumSize - Offset, paint);
                LastUtil.DrawWrappedText($"{album.PlayCount.GetValueOrDefault(0)} Plays", ref canvas, x + Offset,
                    by + AlbumSize - 10,
                    AlbumSize - Offset, paint);
                y = by;
                
                x += 300;
                if (++count % 5 != 0) continue;
                y += 300;
                x = 0;
            }

            return surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).AsStream();
        }
    }
}