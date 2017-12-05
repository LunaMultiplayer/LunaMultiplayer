using Server.Command.Command.Base;

namespace Server.Command.Command
{
    public class WhitelistRemoveCommand : WhitelistCommand
    {
        public override void Execute(string commandArgs)
        {
            Remove(commandArgs);
        }
    }
}