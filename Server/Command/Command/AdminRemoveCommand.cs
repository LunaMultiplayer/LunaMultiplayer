using Server.Command.Command.Base;
using Server.Log;
using Server.System;

namespace Server.Command.Command
{
    public class AdminRemoveCommand : AdminCommand
    {
        public override void Execute(string commandArgs)
        {
            if (Exists(commandArgs))
            {
                LunaLog.Normal($"Removed '{commandArgs}' from the admin list.");
                Remove(commandArgs);
                AdminSystemSender.NotifyPlayersRemovedAdmin(commandArgs);
            }
            else
            {
                LunaLog.Normal($"'{commandArgs}' is not an admin.");
            }
        }
    }
}