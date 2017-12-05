using LMP.Server.Command.Command;
using LMP.Server.Context;
using System;
using System.Threading.Tasks;

namespace LMP.Server
{
    public class EntryPoint
    {
        public static void Stop()
        {
            new ShutDownCommand().Execute("Update found...");
            ServerContext.ServerStarting = false;
            ServerContext.ServerRunning = false;
        }

        public static void MainEntryPoint()
        {
            Task.Run(()=>
            {
                MainServer.Main();
                AppDomain.CurrentDomain.SetData("Stop", true);
            });
        }
    }
}
