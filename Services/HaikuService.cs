using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using static TextStatistics.TextStatistics;

namespace MeiyounaiseSlash.Services
{
    public static class HaikuService
    {
        public static Task DetectHaiku(DiscordClient sender, MessageCreateEventArgs e)
        {
            var message = e.Message.Content.Replace("\n", " ").Split(" ")
                .Select(w => (word: w, sCount: SyllableCount(w))).ToList();
            Console.WriteLine(string.Join(", ", message));

            var lines = new List<string>();
            var toCheck = new[] {5, 7, 5};
            var r = message.Count;
            var i = 0;
            var j = 0;
            var k = 0;
            var sum = 0;
            while (k < r)
            {
                sum += message[i].sCount;
                i++;
                k++;
                if (j >= 3)
                {
                    lines.Clear();
                    break;
                }
                if (sum == toCheck[j])
                {
                    sum = 0;
                    j++;
                    lines.Add(string.Join(" ", message.GetRange(0, i).Select(t => t.word)));
                    message.RemoveRange(0, i);
                    i = 0;
                }
            }

            Console.WriteLine(lines.Count < 3 ? "Not a Haiku" : $"Haiku\n{string.Join("\n\n", lines)}");
            
            return Task.CompletedTask;
        }
    }
}