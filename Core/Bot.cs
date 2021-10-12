using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
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
        public DiscordClient Client { get; set; }
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

            Constants.ErrorLogChannel = _config?.ErrorLogChannel;
            var prometheus = new MetricServer("localhost", 1234);
            prometheus.Start();

            var services = new ServiceCollection()
                .AddSingleton(new BoardDatabase("BoardDatabase.db"))
                .AddSingleton(new GuildDatabase("GuildDatabase.db"))
                .AddSingleton(new LastDatabase("LastDatabase.db"))
                .AddSingleton(new UserDatabase("UserDatabase.db"))
                .BuildServiceProvider();

            var boardService = new BoardService(services.GetService(typeof(BoardDatabase)) as BoardDatabase);
            
            Client = new DiscordClient(new DiscordConfiguration
            {
                Intents = DiscordIntents.All,
                Token = _config?.Token
            });

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(10)
            });
            
            SlashCommands = Client.UseSlashCommands(new SlashCommandsConfiguration
            {
                Services = services
            });

            RegisterHandlers(boardService);
            
            SlashCommands.RegisterCommands<MiscCommands>(328353999508209678);
            SlashCommands.RegisterCommands<Account>(328353999508209678);
            SlashCommands.RegisterCommands<BoardCommands>(328353999508209678);
            
        }

        private void RegisterHandlers(BoardService boardService)
        {
            Client.MessageReactionAdded += boardService.ReactionAdded;
            Client.MessageReactionRemoved += boardService.ReactionRemoved;
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