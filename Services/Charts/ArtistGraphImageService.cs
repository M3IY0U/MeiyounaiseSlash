using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IF.Lastfm.Core.Objects;
using MeiyounaiseSlash.Utilities;
using SkiaSharp;

namespace MeiyounaiseSlash.Services.Charts
{
    public static class ArtistGraphImageService
    {
        private const int Width = 800;
        private const int Height = 400;
        private const float Padding = 25;

        public static async Task<Stream> GenerateChart(string artist,
            IReadOnlyCollection<LastTrack> scrobbles)
        {
            using var surface = SKSurface.Create(new SKImageInfo(Width, Height));
            var canvas = surface.Canvas;
            await Task.Run(() =>
            {
                canvas.DrawImage(SKImage.FromEncodedData("artistgraph-bg.png"), 0, 0);

                if (!string.IsNullOrEmpty(artist))
                    scrobbles = scrobbles.Where(s => s.ArtistName == artist).ToList();

                CreateWeekImage(ref canvas, Constants.ChartFont, !string.IsNullOrEmpty(artist), scrobbles);
            });

            return surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).AsStream();
        }

        private static void CreateWeekImage(ref SKCanvas canvas, SKPaint font, bool isArtist,
            IReadOnlyCollection<LastTrack> scrobbles)
        {
            var paint = new SKPaint
            {
                Color = SKColors.Red,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 6,
                IsAntialias = true,
                FilterQuality = SKFilterQuality.High
            };

            var groupedScrobbles = new List<int>();
            for (var i = 7; i >= 0; i--)
                groupedScrobbles.Add(scrobbles.Count(s =>
                    s.TimePlayed.GetValueOrDefault().AddHours(2).Day == DateTime.UtcNow.AddDays(-i).Day));

            var max = groupedScrobbles.Max();
            var min = groupedScrobbles.Min();

            var pathPaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3,
                IsAntialias = true
            };

            var path = new SKPath();

            path.MoveTo(Padding, Height - Padding * 1.5f -
                                 ((float)groupedScrobbles[0] - min) / ((float)max - min) * (Height - Padding * 3f));
            var points = new List<((float x, float y), int count)>();

            var bS = font.TextSize;
            font.TextSize = 18;

            float oldX = 0f, oldY = 0f;

            for (var i = 0; i < groupedScrobbles.Count; i++)
            {
                var count = groupedScrobbles[i];
                var x = (Width - Padding * 2f) / (groupedScrobbles.Count - 1) * i + Padding;
                var y = Height - Padding * 1.5f - ((float)count - min) / ((float)max - min) * (Height - Padding * 3f);

                points.Add(((x, y), count));

                if (i != 0)
                {
                    if (count == groupedScrobbles[i - 1])
                        path.LineTo(x, y);
                    else
                        path.CubicTo(oldX + (x - oldX) / 2f, oldY, x - (x - oldX) / 2f, y, x, y);
                }

                var date = $"{DateTime.Today.AddDays(i - 7):dd/MM}";
                var dateLength = pathPaint.MeasureText(date);
                canvas.DrawText(date, x - dateLength + 10, Height - Padding + 10, font);

                oldX = x;
                oldY = y;
            }

            canvas.DrawPath(path, pathPaint);

            font.TextSize = bS;
            foreach (var ((x, y), count) in points)
            {
                var tw = font.MeasureText($"{count}");
                canvas.DrawPoint(x, y, paint);
                canvas.DrawText($"{count}", x - tw + tw / 2, y - 10, font);
            }

            canvas.DrawText(isArtist ? $"{scrobbles.First().ArtistName}" : "Weekly Scrobbles", Padding,
                Padding + font.FontSpacing / 2, font);
        }
    }
}