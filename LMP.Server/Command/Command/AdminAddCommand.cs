using LMP.Server.Command.Command.Base;
using LMP.Server.Context;
using LMP.Server.Log;
using LMP.Server.System;
using System.IO;

namespace LMP.Server.Command.Command
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