using LunaCommon.Message.Data.Screenshot;
using LunaCommon.Message.Server;
using LunaCommon.Time;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Graphics = System.Drawing.Graphics;

namespace Server.System
{
    public class ScreenshotSystem
    {
        private const string SmallFilePrefix = "small_";
        private static readonly string ScreenshotFolder = Path.Combine(ServerContext.UniverseDirectory, "Screenshots");

        private static readonly ConcurrentDictionary<string, DateTime> LastUploadRequest = new ConcurrentDictionary<string, DateTime>();

        #region Public Methods

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
                if (DateTime.Now - lastTime > TimeSpan.FromMilliseconds(ScreenshotSettings.SettingsStore.MinScreenshotIntervalMs))
                {
                    LastUploadRequest.AddOrUpdate(client.PlayerName, DateTime.Now, (key, existingVal) => DateTime.Now);
                    if (data.Screenshot.DateTaken == 0) data.Screenshot.DateTaken = LunaTime.UtcNow.ToBinary();
                    var fileName = $"{data.Screenshot.DateTaken}.png";
                    if (!File.Exists(fileName))
                    {
                        var fullPath = Path.Combine(playerFolder, fileName);

                        LunaLog.Normal($"Saving screenshot {fileName} ({data.Screenshot.NumBytes} bytes) from: {client.PlayerName}.");
                        FileHandler.WriteToFile(fullPath, data.Screenshot.Data, data.Screenshot.NumBytes);
                        CreateMiniature(fullPath);
                        SendNotification(client.PlayerName);
                    }
                    else
                    {
                        LunaLog.Warning($"{client.PlayerName} tried to overwrite a screnshot!");
                        return;
                    }
                }
                else
                {
                    LunaLog.Warning($"{client.PlayerName} is sending screenshots too fast!");
                    return;
                }

                //Remove oldest screenshots if the player has too many
                RemovePlayerOldestScreenshots(playerFolder);

                //Checks if we are above the max folders limit
                CheckMaxFolders();
            });
        }

        /// <summary>
        /// Send the screenshot folders that exist on the server
        /// </summary>
        public static void SendScreenshotFolders(ClientStructure client)
        {
            Task.Run(() =>
            {
                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ScreenshotFoldersReplyMsgData>();
                msgData.Folders = Directory.GetDirectories(ScreenshotFolder).Select(d => new DirectoryInfo(d).Name).ToArray();
                msgData.NumFolders = msgData.Folders.Length;

                MessageQueuer.SendToClient<ScreenshotSrvMsg>(client, msgData);
                if (msgData.NumFolders > 0)
                    LunaLog.Debug($"Sending {msgData.NumFolders} screenshot folders to: {client.PlayerName}");
            });
        }

        /// <summary>
        /// Sends the screenshots in a folder
        /// </summary>
        public static void SendScreenshotList(ClientStructure client, ScreenshotListRequestMsgData data)
        {
            Task.Run(() =>
            {
                var screenshots = new List<ScreenshotInfo>();
                var folder = Path.Combine(ScreenshotFolder, data.FolderName);

                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ScreenshotListReplyMsgData>();
                foreach (var file in Directory.GetFiles(folder).Where(f => Path.GetFileNameWithoutExtension(f).StartsWith(SmallFilePrefix)))
                {
                    if (long.TryParse(Path.GetFileNameWithoutExtension(file).Substring(SmallFilePrefix.Length), out var dateTaken))
                    {
                        if (data.AlreadyOwnedPhotoIds.Contains(dateTaken))
                            continue;

                        var bitmap = new Bitmap(file);
                        var contents = File.ReadAllBytes(file);
                        screenshots.Add(new ScreenshotInfo
                        {
                            Data = contents,
                            DateTaken = dateTaken,
                            NumBytes = contents.Length,
                            Height = (ushort)bitmap.Height,
                            Width = (ushort)bitmap.Width,
                            FolderName = data.FolderName,
                        });
                    }
                }

                msgData.FolderName = data.FolderName;
                msgData.Screenshots = screenshots.ToArray();
                msgData.NumScreenshots = screenshots.Count;

                MessageQueuer.SendToClient<ScreenshotSrvMsg>(client, msgData);
                if (msgData.NumScreenshots > 0)
                    LunaLog.Debug($"Sending {msgData.NumScreenshots} ({data.FolderName}) screenshots to: {client.PlayerName}");
            });
        }

        /// <summary>
        /// Sends the requested screenshot
        /// </summary>
        public static void SendScreenshot(ClientStructure client, ScreenshotDownloadRequestMsgData data)
        {
            Task.Run(() =>
            {
                var file = Path.Combine(ScreenshotFolder, data.FolderName, $"{data.DateTaken}.png");
                if (File.Exists(file))
                {
                    var bitmap = new Bitmap(file);

                    var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ScreenshotDataMsgData>();
                    msgData.Screenshot.DateTaken = data.DateTaken;
                    msgData.Screenshot.Data = File.ReadAllBytes(file);
                    msgData.Screenshot.NumBytes = msgData.Screenshot.Data.Length;
                    msgData.Screenshot.Height = (ushort)bitmap.Height;
                    msgData.Screenshot.Width = (ushort)bitmap.Width;
                    msgData.Screenshot.FolderName = data.FolderName;

                    LunaLog.Debug($"Sending screenshot ({msgData.Screenshot.NumBytes} bytes): {data.DateTaken} to: {client.PlayerName}.");
                    MessageQueuer.SendToClient<ScreenshotSrvMsg>(client, msgData);
                }
            });
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Sends a notification of new screenshot to all players
        /// </summary>
        private static void SendNotification(string folderName)
        {
            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ScreenshotNotificationMsgData>();
            msgData.FolderName = folderName;

            MessageQueuer.SendToAllClients<ScreenshotSrvMsg>(msgData);
        }

        /// <summary>
        /// Checks if we have too many player folders and if so, it deletes the oldest one
        /// </summary>
        private static void CheckMaxFolders()
        {
            while (Directory.GetDirectories(ScreenshotFolder).Length > ScreenshotSettings.SettingsStore.MaxScreenshotsFolders)
            {
                var oldestFolder = Directory.GetDirectories(ScreenshotFolder).Select(d => new DirectoryInfo(d)).OrderBy(d => d.LastWriteTime).FirstOrDefault();
                if (oldestFolder != null)
                {
                    LunaLog.Debug($"Removing oldest screenshot folder {oldestFolder.Name}");
                    Directory.Delete(oldestFolder.FullName, true);
                }
            }
        }

        /// <summary>
        /// If the player has too many screenshots this method will remove the oldest ones
        /// </summary>
        private static void RemovePlayerOldestScreenshots(string playerFolder)
        {
            while (new DirectoryInfo(playerFolder).GetFiles().Where(f => !f.Name.StartsWith(SmallFilePrefix)).Count() > ScreenshotSettings.SettingsStore.MaxScreenshotsPerUser)
            {
                var oldestScreenshot = new DirectoryInfo(playerFolder).GetFiles().Where(f => !f.Name.StartsWith(SmallFilePrefix)).OrderBy(f => f.LastWriteTime).FirstOrDefault();
                if (oldestScreenshot != null)
                {
                    LunaLog.Debug($"Deleting old screenshot {oldestScreenshot.FullName}");
                    File.Delete(oldestScreenshot.FullName);
                    File.Delete(Path.Combine(ScreenshotFolder, playerFolder, SmallFilePrefix + oldestScreenshot.Name));
                }
            }
        }

        /// <summary>
        /// Creates a miniature of 120x120 pixels of the given picture
        /// </summary>
        private static void CreateMiniature(string path)
        {
            var fileName = Path.GetFileName(path);
            using (var image = Image.FromFile(path))
            using (var newImage = ScaleImage(image, 120, 120))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                newImage.Save(Path.Combine(Path.GetDirectoryName(path), $"{SmallFilePrefix}{fileName}"), ImageFormat.Png);
            }
        }

        /// <summary>
        /// Scales a given image
        /// </summary>
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

        #endregion
    }
}
