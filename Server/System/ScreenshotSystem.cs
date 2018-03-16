using LunaCommon;
using LunaCommon.Message.Data.Screenshot;
using Server.Client;
using Server.Context;
using Server.Log;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace Server.System
{
    public class ScreenshotSystem
    {
        private static readonly string ScreenshotFolder = Path.Combine(ServerContext.UniverseDirectory, "Screenshots");

        private static readonly ConcurrentDictionary<string, DateTime> LastUploadRequest = new ConcurrentDictionary<string, DateTime>();

        /// <summary>
        /// Saves a received screenshot and creates the miniature
        /// </summary>
        public static void SaveScreenshot(ClientStructure client, ScreenshotDataMsgData data)
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
                    var fileName = $"{DateTime.FromBinary(data.Screenshot.DateTaken):yyyyMMddHHmmss}.png";
                    if (!File.Exists(fileName))
                    {
                        var fullPath = Path.Combine(playerFolder, fileName);

                        LunaLog.Normal($"Saving screenshot from: {client.PlayerName}. Size: {data.Screenshot.NumBytes} bytes. Filename: {fileName}");
                        FileHandler.WriteToFile(fullPath, data.Screenshot.Data, data.Screenshot.NumBytes);
                        CreateMiniature(fullPath);
                    }
                    else
                    {
                        LunaLog.Warning($"{client.PlayerName} tried to overwrite a screnshot!");
                    }
                }
                else
                {
                    LunaLog.Warning($"{client.PlayerName} is sending screenshots too fast!");
                }
            });
        }


        private static void CreateMiniature(string path)
        {
            var fileName = Path.GetFileName(path);
            using (var image = Image.FromFile(path))
            using (var newImage = ScaleImage(image, 120, 120))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                newImage.Save(Path.Combine(Path.GetDirectoryName(path), $"small_{fileName}"), ImageFormat.Png);
            }
        }

        private static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }
    }
}
