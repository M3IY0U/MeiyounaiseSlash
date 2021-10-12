using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using MeiyounaiseSlash.Utilities;

namespace MeiyounaiseSlash.Exceptions
{
    public static class ExceptionHandler
    {
        public static async Task SlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs args)
        {
            string content;
            switch (args.Exception)
            {
                case CommandException:
                    content = $"❌ {args.Exception.Message}";
                    break;

                case InteractionTimeoutException:
                    content = $"💤 {args.Exception.Message}";
                    break;
                case SlashExecutionChecksFailedException:
                    try
                    {
                        await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
                                .WithDescription("🚫 You don't have permission to use that command.")));
                    }
                    catch (Exception) { /* ignored */ }
                    return;
                default:
                    content = "❌ Internal Exception";

                    if (Constants.ErrorLogChannel.HasValue)
                    {
                        var channel = await args.Context.Client.GetChannelAsync(Constants.ErrorLogChannel.Value);
                        await channel.SendMessageAsync(
                            $"\\<{args.Exception.GetType().Name}\\> **Message:** {args.Exception.Message}",
                            new DiscordEmbedBuilder()
                                .WithTitle($"Command \"{args.Context.CommandName}\" errored")
                                .WithDescription(
                                    $"```{args.Exception.StackTrace?[.. Math.Min(args.Exception.StackTrace.Length, 4090)]}```"));
                    }

                    break;
            }

            await args.Context.Interaction.CreateFollowupMessageAsync(
                new DiscordFollowupMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                        .WithDescription(content)));
            Statistics.CommandExceptionCounter.Inc();
        }
    }
}