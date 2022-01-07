using System;
using DSharpPlus.SlashCommands;
using LiteDB;

namespace MeiyounaiseSlash.Data
{
    public static class CommandDatabase
    {
        public class Command
        {
            public string Name { get; set; }
            public ulong User { get; set; }
            public DateTime TimeStamp { get; set; }
            public ulong Channel { get; set; }
        }

        private static LiteDatabase _database = new("CommandLog.db");
        private static ILiteCollection<Command> _commandCollection = _database.GetCollection<Command>("commands");
        
        public static void LogCommand(InteractionContext ctx)
        {
            _commandCollection.Insert(new Command
            {
                Name = ctx.CommandName,
                User = ctx.User.Id,
                TimeStamp = DateTime.Now,
                Channel = ctx.Channel.Id
            });
        }
    }
}