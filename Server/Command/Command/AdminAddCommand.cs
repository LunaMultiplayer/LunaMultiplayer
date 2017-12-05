using System.IO;
using Server.Command.Command.Base;
using Server.Context;
using Server.Log;
using Server.System;

namespace Server.Command.Command
{
    public class AdminAddCommand : AdminCommand
    {
        public override void Execute(string commandArgs)
        {
            if (FileHandler.FileExists(Path.Combine(ServerContext.UniverseDirectory, "Players", $"{commandArgs}.txt")))
            {
                if (Exists(commandArgs)) return;

                LunaLog.Debug($"Added '{commandArgs}' to admin list.");
                Add(commandArgs);
                AdminSystemSender.NotifyPlayersNewAdmin(commandArgs);
            }
            else
            {
                LunaLog.Normal($"'{commandArgs}' does not exist.");
            }
        }
    }
}