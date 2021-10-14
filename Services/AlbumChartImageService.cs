using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using IF.Lastfm.Core.Objects;
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
                y = DrawWrappedText($"{album.ArtistName} -", ref canvas, x + Offset, y + 25, AlbumSize - Offset, paint);
                y = DrawWrappedText($"{album.Name}", ref canvas, x + Offset, y, AlbumSize - Offset, paint);
                DrawWrappedText($"{album.PlayCount.GetValueOrDefault(0)} Plays", ref canvas, x + Offset, y,
                    AlbumSize - Offset, paint);
                y = by;

                x += 300;
                if (++count % 5 != 0) continue;
                y += 300;
                x = 0;
            }

            return surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).AsStream();
        }

        private static float DrawWrappedText(string text, ref SKCanvas canvas, float x, float y, float maxlength,
            SKPaint paint)
        {
            var wrapped = new List<string>();
            var lineLength = 0f;
            var line = "";

            foreach (var word in text.ToCharArray())
            {
                var wordWithSpace = word + "";
                var wordLength = paint.MeasureText(wordWithSpace);
                if (lineLength + wordLength > maxlength)
                {
                    wrapped.Add(line);
                    line = wordWithSpace;
                    lineLength = wordLength;
                }
                else
                {
                    line += wordWithSpace;
                    lineLength += wordLength;
                }
            }

            wrapped.Add(line);

            foreach (var wrappedLine in wrapped)
            {
                canvas.DrawText(wrappedLine, x, y, paint);
                y += paint.TextSize + Offset;
            }

            return y;
        }
    }
}