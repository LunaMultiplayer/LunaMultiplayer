using Server.Command.Command.Base;

namespace Server.Command.Command
{
    public class WhitelistAddCommand : WhitelistCommand
    {
        public override void Execute(string commandArgs)
        {
            Add(commandArgs);
        }
    }
}