using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using MeiyounaiseSlash.Data;

namespace MeiyounaiseSlash.Commands
{
    public class LogCommand : ApplicationCommandModule
    {
        public override Task AfterSlashExecutionAsync(InteractionContext ctx)
        {
            CommandDatabase.LogCommand(ctx);
            return base.AfterSlashExecutionAsync(ctx);
        }
    }
}