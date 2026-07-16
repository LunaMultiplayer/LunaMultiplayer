using LmpCommon.Time;
using Server.Context;
using System.IO;
using System.Threading.Tasks;

namespace Server.Log
{
    public class LogThread
    {
        private static long _lastLogExpiredCheck;
        private static long _lastDayCheck;

        public static async Task RunLogThreadAsync()
        {
            while (ServerContext.ServerRunning)
            {
                //Run the log expire function every 10 minutes
                if (ServerContext.ServerClock.ElapsedMilliseconds - _lastLogExpiredCheck > 600000)
                {
                    _lastLogExpiredCheck = ServerContext.ServerClock.ElapsedMilliseconds;
                    LogExpire.ExpireLogs();
                }

                // Check if the day has changed, every minute
                if (ServerContext.ServerClock.ElapsedMilliseconds - _lastDayCheck > 60000)
                {
                    _lastDayCheck = ServerContext.ServerClock.ElapsedMilliseconds;
                    if (ServerContext.Day != LunaNetworkTime.Now.Day)
                    {
                        LunaLog.LogFilename = Path.Combine(LunaLog.LogFolder, $"lmpserver {LunaNetworkTime.Now:yyyy-MM-dd HH-mm-ss}.log");
                        LunaLog.Info($"Continued from logfile {LunaNetworkTime.Now:yyyy-MM-dd HH-mm-ss}.log");
                        ServerContext.Day = LunaNetworkTime.Now.Day;
                    }
                }

                await Task.Delay(250);
            }
        }
    }
}
