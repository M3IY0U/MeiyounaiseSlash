using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using IF.Lastfm.Core.Api;
using MeiyounaiseSlash.Data;
using MeiyounaiseSlash.Data.Repositories;
using MeiyounaiseSlash.Exceptions;
using MeiyounaiseSlash.Services;
using MeiyounaiseSlash.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SpotifyAPI.Web;

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

            var services = new ServiceCollection()
                .AddSingleton(new BoardDatabase("BoardDatabase.db"))
                .AddSingleton(new SpotifyClient(SpotifyClientConfig.CreateDefault()
                    .WithAuthenticator(new ClientCredentialsAuthenticator(
                        _config.SpotifyClientId,
                        _config.SpotifyClientSecret))))
                .AddSingleton(new LastfmClient(_config.LastApiKey, _config.LastApiSecret))
                .AddDbContext<MeiyounaiseContext>(options =>
                    options.EnableSensitiveDataLogging().UseNpgsql(_config.ConnectionString))
                .AddScoped<ScrobbleRepository>()
                .AddScoped<GuildRepository>()
                .AddScoped<UserRepository>()
                .BuildServiceProvider();

            //services.GetService<MeiyounaiseContext>()?.Database.EnsureDeleted();
            services.GetService<MeiyounaiseContext>()?.Database.EnsureCreated();

            var boardService = new BoardService(services.GetService(typeof(BoardDatabase)) as BoardDatabase);
            var guildService = new GuildService(services.GetService<GuildRepository>());


            Client = new DiscordClient(new DiscordConfiguration
            {
                Intents = DiscordIntents.All,
                Token = _config.Token
            });

            Client.UseInteractivity(new InteractivityConfiguration
            {
                AckPaginationButtons = true
            });

            SlashCommands = Client.UseSlashCommands(new SlashCommandsConfiguration
            {
                Services = services
            });

            RegisterHandlers(boardService, guildService);

            SlashCommands.RegisterCommands(Assembly.GetEntryAssembly(), 328353999508209678);
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