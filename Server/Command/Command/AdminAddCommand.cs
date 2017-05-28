using LunaServer.Command.Command.Base;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.System;
using System.IO;

namespace LunaServer.Command.Command
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