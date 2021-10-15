using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IF.Lastfm.Core.Objects;
using MeiyounaiseSlash.Commands.Last;
using MeiyounaiseSlash.Utilities;
using SkiaSharp;

namespace MeiyounaiseSlash.Services
{
    public static class ArtistChartImageService
    {
        private const int Offset = 5;
        private const float ArtistSize = 300f;

        public static async Task<Stream> GenerateChart(IReadOnlyList<LastArtist> items)
        {
            var images = await Task.WhenAll(items.Select(LastUtil.ScrapeImageAsync));

            using var surface =
                SKSurface.Create(new SKImageInfo((int) Math.Min(items.Count * ArtistSize, 1500),
                    (int) (((items.Count - 1) / 5 + 1) * ArtistSize)));
            var canvas = surface.Canvas;
            var paint = Constants.ChartFont;

            canvas.Clear(SKColors.Transparent);

            var x = 0f;
            var y = 0f;
            var count = 0;
            var wc = new WebClient();
            foreach (var (artist, imageUrl) in images)
            {
                var img = SKImage.FromEncodedData(wc.DownloadData(imageUrl ?? Constants.LastFmUnknownArtist));
                
                var s = Math.Min(img.Width, img.Height);
                
                canvas.DrawImage(img, SKRect.Create((img.Width - s) / 2f, (img.Height - s) / 2f, s, s),
                    SKRect.Create(x, y, ArtistSize, ArtistSize));

                LastUtil.DrawWrappedText(artist.Name, ref canvas, x + Offset, y + 25,
                    ArtistSize - Offset, paint);
                LastUtil.DrawWrappedText($"{artist.PlayCount.GetValueOrDefault(0)} Plays", ref canvas,
                    x + Offset,
                    y + ArtistSize - 10,
                    ArtistSize - Offset, paint);

                x += 300;
                if (++count % 5 != 0) continue;
                y += 300;
                x = 0;
            }

            return surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).AsStream();
        }
    }
}