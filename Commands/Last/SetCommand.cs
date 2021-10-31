using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MeiyounaiseSlash.Data.Repositories;
using MeiyounaiseSlash.Exceptions;
using MeiyounaiseSlash.Utilities;

namespace MeiyounaiseSlash.Commands.Last
{
    [SlashCommandGroup("set", "Set various last.fm related info.")]
    public class SetCommand : LogCommand
    {
        public UserRepository UserRepository { get; set; }

        [SlashCommand("last", "Set your last.fm account.")]
        public async Task SetLast(InteractionContext ctx,
            [Option("lastfm", "Your last.fm account name")]
            string last = "")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));

            string content;
            if (string.IsNullOrEmpty(last))
            {
                content = await UserRepository.TryGetLast(ctx.User.Id, out last)
                    ? $"{Constants.InfoEmoji} Your last.fm account is currently set to: `{last}`"
                    : $"{Constants.ErrorEmoji} You have not set your last.fm account yet.";
            }
            else
            {
                await UserRepository.SetLastAccount(ctx.User.Id, last);
                content = $"{Constants.CheckEmoji} Your last.fm account has been set to: `{last}`";
            }

            await ctx.EditResponseAsync(Util.EmbedReply(content));
        }

        [SlashCommand("reaction", "Set your last.fm account.")]
        public async Task ManageReactions(InteractionContext ctx,
            [Option("action", "Action to take")] ReactChoice action,
            [Option("reaction", "Reaction to use")]
            string reaction = "")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            switch (action)
            {
                case ReactChoice.add:
                    if (string.IsNullOrEmpty(reaction))
                        throw new CommandException(
                            "You need to provide an emoji to add.");

                    if (DiscordEmoji.TryFromName(ctx.Client, $":{reaction.Split(':')[1]}:", out var emoji))
                    {
                        await UserRepository.AddReaction(ctx.User.Id, emoji.GetDiscordName());
                        await ctx.EditResponseAsync(
                            Util.EmbedReply($"{Constants.CheckEmoji} Reaction {emoji} was added."));
                    }
                    else
                        throw new CommandException("Provided emoji is not available or could not be parsed.");

                    break;
                case ReactChoice.clear:
                    await UserRepository.ClearReactions(ctx.User.Id);
                    await ctx.EditResponseAsync(
                        Util.EmbedReply($"{Constants.CheckEmoji} Your reactions were cleared."));
                    break;
                case ReactChoice.list:
                    var reactions = await UserRepository.GetReactions(ctx.User.Id);
                    await ctx.EditResponseAsync(Util.EmbedReply(
                        $"{Constants.InfoEmoji} Currently reacting with: " + string.Join(" ",
                            reactions.Select(r => DiscordEmoji.FromName(ctx.Client, r)))));
                    break;
                default:
                    throw new CommandException("Invalid option provided.");
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum ReactChoice
        {
            [ChoiceName("add")] add,
            [ChoiceName("clear")] clear,
            [ChoiceName("list")] list
        }
    }
}