using LMP.Server.Command.Command.Base;

namespace LMP.Server.Command.Command
{
    public class WhitelistAddCommand : WhitelistCommand
    {
        public override void Execute(string commandArgs)
        {
            Add(commandArgs);
        }
    }
}