using LmpClient.Base;
using LmpClient.Localization;
using LmpClient.Systems.SettingsSys;
using LmpClient.Utilities;
using LmpCommon.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LmpClient.Systems.Screenshot
{
    public class ScreenshotSystem : MessageSystem<ScreenshotSystem, ScreenshotMessageSender, ScreenshotMessageHandler>
    {
        #region Fields and properties

        private static readonly string ScreenshotsFolder = CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Screenshots");
        private static DateTime _lastTakenScreenshot = DateTime.MinValue;
        public ConcurrentDictionary<string, ConcurrentDictionary<long, Screenshot>> MiniatureImages { get; } = new ConcurrentDictionary<string, ConcurrentDictionary<long, Screenshot>>();
        public ConcurrentDictionary<string, ConcurrentDictionary<long, Screenshot>> DownloadedImages { get; } = new ConcurrentDictionary<string, ConcurrentDictionary<long, Screenshot>>();
        public List<string> FoldersWithNewContent { get; } = new List<string>();
        public bool NewContent => FoldersWithNewContent.Any();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(ScreenshotSystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            MessageSender.RequestFolders();
            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, CheckScreenshots));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            MiniatureImages.Clear();
            DownloadedImages.Clear();
            FoldersWithNewContent.Clear();
        }

        #endregion

        /// <summary>
        /// Checks and sends if we took a screenshot
        /// </summary>
        public void CheckScreenshots()
        {
            if (GameSettings.TAKE_SCREENSHOT.GetKeyDown())
            {
                if (TimeUtil.IsInInterval(ref _lastTakenScreenshot, SettingsSystem.ServerSettings.MinScreenshotIntervalMs))
                {
                    var path = CommonUtil.CombinePaths(MainSystem.KspPath, "Screenshots");
                    CoroutineUtil.StartDelayedRoutine(nameof(CheckScreenshots), () =>
                    {
                        var photo = new DirectoryInfo(path).GetFiles().OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                        if (photo != null)
                        {
                            var imageData = ScaleScreenshot(File.ReadAllBytes(photo.FullName), 800, 600);
                            TaskFactory.StartNew(() =>
                            {
                                MessageSender.SendScreenshot(imageData);
                            });
                            LunaScreenMsg.PostScreenMessage(LocalizationContainer.ScreenText.ScreenshotTaken, 10f, ScreenMessageStyle.UPPER_CENTER);
                        }
                    }, 0.3f);
                }
                else
                {
                    var msg = LocalizationContainer.ScreenText.ScreenshotInterval.Replace("$1", TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.MinScreenshotIntervalMs).TotalSeconds
                            .ToString(CultureInfo.InvariantCulture));

                    LunaScreenMsg.PostScreenMessage(msg, 20f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
        }

        /// <summary>
        /// Saves the requested image to disk
        /// </summary>
        public void SaveImage(string folder, long dateTaken)
        {
            if (DownloadedImages.TryGetValue(folder, out var downloadedImages) && downloadedImages.TryGetValue(dateTaken, out var image))
            {
                var folderPath = CommonUtil.CombinePaths(ScreenshotsFolder, folder);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var filePath = CommonUtil.CombinePaths(folderPath, $"{dateTaken}.png");
                File.WriteAllBytes(filePath, image.Data);
                LunaScreenMsg.PostScreenMessage(LocalizationContainer.ScreenText.ImageSaved, 20f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        /// <summary>
        /// Requests the miniatures if the folder is empty or there are new screenshots
        /// </summary>
        public void RequestMiniaturesIfNeeded(string selectedFolder)
        {
            if (FoldersWithNewContent.Contains(selectedFolder))
            {
                FoldersWithNewContent.Remove(selectedFolder);
                MessageSender.RequestMiniatures(selectedFolder);
                return;
            }

            if (MiniatureImages.GetOrAdd(selectedFolder, new ConcurrentDictionary<long, Screenshot>()).Count == 0)
                MessageSender.RequestMiniatures(selectedFolder);
        }

        private static byte[] ScaleScreenshot(byte[] source, int maxWidth, int maxHeight)
        {
            var image = new Texture2D(1, 1);
            image.LoadImage(source);

            var ratioX = (double)maxWidth / image.width;
            var ratioY = (double)maxHeight / image.height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.width * ratio);
            var newHeight = (int)(image.height * ratio);

            var scaledImage = new Texture2D(newWidth, newHeight);
            for (var i = 0; i < scaledImage.height; ++i)
            {
                for (var j = 0; j < scaledImage.width; ++j)
                {
                    var newColor = image.GetPixelBilinear(j / (float)scaledImage.width, i / (float)scaledImage.height);
                    scaledImage.SetPixel(j, i, newColor);
                }
            }

            scaledImage.Apply();
            return scaledImage.EncodeToPNG();
        }
    }
}
