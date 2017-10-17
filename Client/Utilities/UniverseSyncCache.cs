using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace LunaClient.Utilities
{
    public class UniverseSyncCache
    {
        static UniverseSyncCache()
        {
            SystemBase.LongRunTaskFactory.StartNew(ProcessingThreadMain);
        }

        #region Fields

        private static bool _stop;
        public static long CurrentCacheSize { get; private set; }

        private static string CacheDirectory => CommonUtil.CombinePaths(Client.KspPath, "GameData", "LunaMultiPlayer", "Cache");

        private static AutoResetEvent IncomingEvent { get; } = new AutoResetEvent(false);
        private static ConcurrentQueue<byte[]> IncomingQueue { get; } = new ConcurrentQueue<byte[]>();
        private static Dictionary<string, long> FileLengths { get; } = new Dictionary<string, long>();
        private static Dictionary<string, DateTime> FileCreationTimes { get; } = new Dictionary<string, DateTime>();

        #endregion

        #region Public methods

        public static void Stop()
        {
            _stop = true;
            Thread.Sleep(500);
        }

        public static string[] GetCachedObjects()
        {
            if (SettingsSystem.CurrentSettings.EnableCache)
            {
                var cacheFiles = GetCachedFiles();
                var cacheObjects = new string[cacheFiles.Length];
                for (var i = 0; i < cacheFiles.Length; i++)
                    cacheObjects[i] = Path.GetFileNameWithoutExtension(cacheFiles[i]);
                return cacheObjects;
            }

            return new string[0];
        }

        /// <summary>
        /// Delete old cache files or if size is bigger than the limit.
        /// </summary>
        public static void ExpireCache()
        {
            LunaLog.Log("[LMP]: Expiring cache!");

            //No folder, no delete.
            if (!Directory.Exists(CommonUtil.CombinePaths(CacheDirectory, "Incoming")))
            {
                LunaLog.Log("[LMP]: No sync cache folder, skipping expire.");
                return;
            }

            //Delete partial incoming files
            var incomingFiles = Directory.GetFiles(CommonUtil.CombinePaths(CacheDirectory, "Incoming"));
            foreach (var incomingFile in incomingFiles)
            {
                LunaLog.Log($"[LMP]: Deleting partially cached object {incomingFile}");
                File.Delete(incomingFile);
            }

            //Delete old files
            var cacheObjects = GetCachedObjects();
            CurrentCacheSize = 0;
            foreach (var cacheObject in cacheObjects)
            {
                var cacheFile = CommonUtil.CombinePaths(CacheDirectory, $"{cacheObject}.txt");
                //If the file is older than a week, delete it.
                if (File.GetCreationTime(cacheFile).AddDays(7d) < DateTime.Now)
                {
                    LunaLog.Log($"[LMP]: Deleting cached object {cacheObject}, reason: Expired!");
                    File.Delete(cacheFile);
                }
                else
                {
                    var fi = new FileInfo(cacheFile);
                    FileCreationTimes[cacheObject] = fi.CreationTime;
                    FileLengths[cacheObject] = fi.Length;
                    CurrentCacheSize += fi.Length;
                }
            }

            //While the directory is over (cacheSize) MB
            while (CacheSizeExceeded())
            {
                var deleteObject = string.Empty;
                //Find oldest file
                foreach (var testFile in FileCreationTimes)
                {
                    if (string.IsNullOrEmpty(deleteObject))
                        deleteObject = testFile.Key;
                    if (testFile.Value < FileCreationTimes[deleteObject])
                        deleteObject = testFile.Key;
                }

                LunaLog.Log($"[LMP]: Deleting cached object {deleteObject}, reason: Cache full!");
                var deleteFile = CommonUtil.CombinePaths(CacheDirectory, $"{deleteObject}.txt");
                File.Delete(deleteFile);

                CurrentCacheSize -= FileLengths[deleteObject];

                if (FileCreationTimes.ContainsKey(deleteObject))
                    FileCreationTimes.Remove(deleteObject);
                if (FileLengths.ContainsKey(deleteObject))
                    FileLengths.Remove(deleteObject);
            }
        }

        /// <summary>
        /// Queues to cache. This method is non-blocking, use SaveToCache for a blocking method.
        /// </summary>
        public static void QueueToCache(byte[] fileData)
        {
            if (SettingsSystem.CurrentSettings.EnableCache)
            {
                IncomingQueue.Enqueue(fileData);
                IncomingEvent.Set();
            }
        }

        /// <summary>
        /// Tries to get an object from cache
        /// </summary>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public static byte[] GetFromCache(string objectName)
        {
            var objectFile = CommonUtil.CombinePaths(CacheDirectory, $"{objectName}.txt");
            if (File.Exists(objectFile))
                return File.ReadAllBytes(objectFile);
            throw new IOException($"Cached object {objectName} does not exist");
        }

        /// <summary>
        /// Deletes all cache files
        /// </summary>
        public static void DeleteCache()
        {
            LunaLog.Log("[LMP]: Deleting cache!");
            foreach (var cacheFile in GetCachedFiles())
                File.Delete(cacheFile);
            FileLengths.Clear();
            FileCreationTimes.Clear();
            CurrentCacheSize = 0;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Processes the queued objects and save them as files in async mode
        /// </summary>
        private static void ProcessingThreadMain()
        {
            while (!_stop && SettingsSystem.CurrentSettings.EnableCache)
            {
                if (IncomingQueue.TryDequeue(out var incomingBytes))
                    SaveToCache(incomingBytes);
                else
                    IncomingEvent.WaitOne(500);
            }
        }

        private static bool CacheSizeExceeded()
        {
            return CurrentCacheSize > SettingsSystem.CurrentSettings.CacheSize * 1024 * 1024;
        }

        private static string[] GetCachedFiles()
        {
            return Directory.GetFiles(CacheDirectory);
        }

        private static void SaveToCache(byte[] fileData)
        {
            if (fileData == null || fileData.Length == 0 || CacheSizeExceeded())
                return;

            var objectName = Common.CalculateSha256Hash(fileData);
            var objectFile = CommonUtil.CombinePaths(CacheDirectory, $"{objectName}.txt");
            var incomingFile = CommonUtil.CombinePaths(CacheDirectory, "Incoming", $"{objectName}.txt");
            if (!File.Exists(objectFile))
            {
                File.WriteAllBytes(incomingFile, fileData);
                File.Move(incomingFile, objectFile);
                CurrentCacheSize += fileData.Length;
                FileLengths[objectName] = fileData.Length;
                FileCreationTimes[objectName] = new FileInfo(objectFile).CreationTime;
            }
            else
            {
                File.SetCreationTime(objectFile, DateTime.Now);
                FileCreationTimes[objectName] = new FileInfo(objectFile).CreationTime;
            }
        }

        #endregion
    }
}