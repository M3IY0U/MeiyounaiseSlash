using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using IF.Lastfm.Core.Api;
using MeiyounaiseSlash.Commands;
using MeiyounaiseSlash.Commands.Last;
using MeiyounaiseSlash.Data;
using MeiyounaiseSlash.Exceptions;
using MeiyounaiseSlash.Services;
using MeiyounaiseSlash.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Prometheus;

namespace MeiyounaiseSlash.Core
{
    public class Bot : IDisposable
    {
        public DiscordClient Client { get; }
        public SlashCommandsExtension SlashCommands { get; set; }
        private readonly Config _config;


        public Bot()
        {
            if (File.Exists("config.json"))
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            else
            {
                Console.WriteLine($"Config missing in: {Directory.GetCurrentDirectory()}");
                Environment.Exit(1);
            }

            if (_config is null)
                throw new Exception("Error loading config.");

            Constants.ErrorLogChannel = _config.ErrorLogChannel;
            var prometheus = new MetricServer("localhost", 1234);
            prometheus.Start();

            var services = new ServiceCollection()
                .AddSingleton(new BoardDatabase("BoardDatabase.db"))
                .AddSingleton(new GuildDatabase("GuildDatabase.db"))
                .AddSingleton(new LastDatabase("LastDatabase.db"))
                .AddSingleton(new UserDatabase("UserDatabase.db"))
                .AddSingleton(new LastfmClient(_config.LastApiKey, _config.LastApiSecret))
                .BuildServiceProvider(true);

            var boardService = new BoardService(services.GetService(typeof(BoardDatabase)) as BoardDatabase);
            var guildService = new GuildService(services.GetService(typeof(GuildDatabase)) as GuildDatabase);

            Client = new DiscordClient(new DiscordConfiguration
            {
                Intents = DiscordIntents.All,
                Token = _config.Token
            });

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(10)
            });

            SlashCommands = Client.UseSlashCommands(new SlashCommandsConfiguration
            {
                Services = services
            });

            RegisterHandlers(boardService, guildService);

            SlashCommands.RegisterCommands<MiscCommands>(328353999508209678);
            SlashCommands.RegisterCommands<Account>(328353999508209678);
            SlashCommands.RegisterCommands<BoardCommands>(328353999508209678);
            SlashCommands.RegisterCommands<GuildCommands>(328353999508209678);
            SlashCommands.RegisterCommands<NowPlaying>(328353999508209678);
            SlashCommands.RegisterCommands<AlbumChart>(328353999508209678);
            SlashCommands.RegisterCommands<ArtistChart>(328353999508209678);
            SlashCommands.RegisterCommands<ContextMenuActions>(328353999508209678);
            SlashCommands.RegisterCommands<Recent>(328353999508209678);
            SlashCommands.RegisterCommands<Server>(328353999508209678);
        }

        private void RegisterHandlers(BoardService boardService, GuildService guildService)
        {
            Client.MessageReactionAdded += boardService.ReactionAdded;
            Client.MessageReactionRemoved += boardService.ReactionRemoved;
            Client.GuildMemberAdded += guildService.MemberJoined;
            Client.GuildMemberRemoved += guildService.MemberLeft;
            Client.MessageCreated += guildService.RepeatMessage;
            SlashCommands.SlashCommandErrored += ExceptionHandler.SlashCommandErrored;
        }

        public void Dispose()
        {
            Client.Dispose();
            SlashCommands = null;
            GC.SuppressFinalize(this);
        }

        public async Task RunAsync()
        {
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}