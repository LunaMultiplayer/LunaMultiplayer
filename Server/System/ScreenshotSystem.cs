using LunaCommon;
using LunaCommon.Message.Data.Screenshot;
using Server.Client;
using Server.Context;
using Server.Log;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Server.System
{
    public class ScreenshotSystem
    {
        private static readonly Random Random = new Random();
        private static readonly string ScreenshotFolder = Path.Combine(ServerContext.UniverseDirectory, "Screenshots");

        private static readonly ConcurrentDictionary<string, DateTime> LastUploadRequest = new ConcurrentDictionary<string, DateTime>();

        public static void SaveScreenshot(ClientStructure client, ScreenshotUploadMsgData data)
        {
            Task.Run(() =>
            {
                var playerFolder = Path.Combine(ScreenshotFolder, client.PlayerName);
                if (!Directory.Exists(playerFolder))
                {
                    Directory.CreateDirectory(playerFolder);
                }

                var lastTime = LastUploadRequest.GetOrAdd(client.PlayerName, DateTime.MinValue);
                if (DateTime.Now - lastTime > TimeSpan.FromMilliseconds(CommonConstants.MinScreenshotMsInterval))
                {
                    var filename = $"{Random.Next(1000000)}_{DateTime.Now:yyyyMMddHHmmss}.png";
                    LunaLog.Normal($"Saving screenshot from: {client.PlayerName}. Size: {data.NumBytes} bytes. Filename: {filename}");
                    FileHandler.WriteToFile(Path.Combine(playerFolder, filename), data.Data, data.NumBytes);
                }
                else
                {
                    LunaLog.Warning($"{client.PlayerName} is sending screenshots too fast!");
                }
            });
        }
    }
}
