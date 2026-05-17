using Server.Command.Command.Base;
using Server.Log;
using Server.System;

namespace Server.Command.Command
{
    public class BackupCommand : SimpleCommand
    {
        public override bool Execute(string commandArgs)
        {
            var args = commandArgs.Trim().ToLower();

            if (string.IsNullOrEmpty(args) || args == "now")
            {
                LunaLog.Normal("Manual backup initiated...");
                BackupSystem.RunBackup();
                LunaLog.Normal("Manual backup completed successfully.");
                return true;
            }

            LunaLog.Normal("Usage: /backup [now]");
            return false;
        }
    }
}
