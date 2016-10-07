using LunaServer.Command.Command.Base;

namespace LunaServer.Command.Command
{
    public class WhitelistRemoveCommand : WhitelistCommand
    {
        public override void Execute(string commandArgs)
        {
            Remove(commandArgs);
        }
    }
}