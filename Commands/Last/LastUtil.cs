using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Objects;
using MeiyounaiseSlash.Utilities;
using SkiaSharp;

namespace MeiyounaiseSlash.Commands.Last
{
    public static class LastUtil
    {
        public enum TimeSpan
        {
            [ChoiceName("overall")] Overall,
            [ChoiceName("year")] Year,
            [ChoiceName("half")] Half,
            [ChoiceName("quarter")] Quarter,
            [ChoiceName("month")] Month,
            [ChoiceName("week")] Week
        }
        
        public static LastStatsTimeSpan EnumToTimeSpan(TimeSpan timespan)
        {
            return timespan switch
            {
                TimeSpan.Week => LastStatsTimeSpan.Week,
                TimeSpan.Month => LastStatsTimeSpan.Month,
                TimeSpan.Quarter => LastStatsTimeSpan.Quarter,
                TimeSpan.Half=> LastStatsTimeSpan.Half,
                TimeSpan.Year=> LastStatsTimeSpan.Year,
                _ => LastStatsTimeSpan.Overall
            };
        }
        
        public static float DrawWrappedText(string text, ref SKCanvas canvas, float x, float y, float maxlength,
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
                y += paint.TextSize + 5;
            }

            return y;
        }

        public static async Task<(LastArtist artist, string imageUrl)> ScrapeImageAsync(LastArtist artist)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(artist.Url);
            var result = await response.Content.ReadAsStringAsync();
            try
            {
                result = result.Substring(
                    result.IndexOf("<meta property=\"og:image\"           content=\"", StringComparison.Ordinal) +
                    45,
                    150);
                result = result.Remove(result.IndexOf("\"", StringComparison.Ordinal));
            }
            catch (Exception)
            {
                result = Constants.LastFmUnknownArtist;
            }

            return (artist, result);
        }

        public static async Task<string> ScrapeImageAsync(string artistUrl)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(artistUrl);
            var result = await response.Content.ReadAsStringAsync();
            try
            {
                result = result.Substring(
                    result.IndexOf("<meta property=\"og:image\"           content=\"", StringComparison.Ordinal) +
                    45,
                    150);
                return result.Remove(result.IndexOf("\"", StringComparison.Ordinal));
            }
            catch (Exception)
            {
                return Constants.LastFmUnknownArtist;
            }
        }

        public struct NowPlayingStruct
        {
            public ulong Id { get; set; }
            public string Last { get; set; }
            public LastTrack Track { get; set; }
        }

        public static async Task<NowPlayingStruct> GetNowPlaying(ulong id,
            string user,
            LastfmClient client)
        {
            try
            {
                var response = await client.User.GetRecentScrobbles(user);
                if (!response.Success)
                    return new NowPlayingStruct();

                var track = response.Content[0];
                if (track?.IsNowPlaying is null || !track.IsNowPlaying.Value)
                    return new NowPlayingStruct();

                return new NowPlayingStruct
                {
                    Id = id,
                    Last = user,
                    Track = track
                };
            }
            catch (Exception)
            {
                return new NowPlayingStruct();
            }
        }

        public static Uri CleanLastUrl(string url)
            => new(url.Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace(" ", "+")
                .Replace("　", "%E3%80%80"));

        public static Uri CleanLastUrl(Uri uri)
            => CleanLastUrl(uri.ToString());
    }
}