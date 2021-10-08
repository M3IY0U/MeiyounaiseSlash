using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using MeiyounaiseSlash.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

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

            var services = new ServiceCollection().BuildServiceProvider();
            
            Client = new DiscordClient(new DiscordConfiguration
            {
                Intents = DiscordIntents.All,
                Token = _config?.Token
            });

            SlashCommands = Client.UseSlashCommands(new SlashCommandsConfiguration
            {
                Services = services
            });

            SlashCommands.SlashCommandErrored += (sender, args) =>
            {
                args.Context.Channel.SendMessageAsync(
                    $"Error: `{args.Exception.Message}`\n```{args.Exception.StackTrace}```");
                return Task.CompletedTask;
            };
            
            SlashCommands.RegisterCommands<MiscCommands>(328353999508209678);
            
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