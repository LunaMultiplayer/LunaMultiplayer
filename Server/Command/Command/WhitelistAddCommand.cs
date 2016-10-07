using LunaServer.Command.Command.Base;

namespace LunaServer.Command.Command
{
    public class WhitelistAddCommand : WhitelistCommand
    {
        public override void Execute(string commandArgs)
        {
            Add(commandArgs);
        }
    }
}