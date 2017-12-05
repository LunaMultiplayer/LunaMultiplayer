using LMP.Server.Command.Command.Base;

namespace LMP.Server.Command.Command
{
    public class WhitelistRemoveCommand : WhitelistCommand
    {
        public override void Execute(string commandArgs)
        {
            Remove(commandArgs);
        }
    }
}