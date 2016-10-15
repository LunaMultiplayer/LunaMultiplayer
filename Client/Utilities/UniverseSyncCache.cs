using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LunaClient.Systems.SettingsSys;
using LunaCommon;
using UnityEngine;

namespace LunaClient.Utilities
{
    public class UniverseSyncCache
    {
        public UniverseSyncCache()
        {
            var processingThread = new Thread(ProcessingThreadMain) {IsBackground = true};
            processingThread.Start();
        }

        #region Fields

        public static UniverseSyncCache Singleton { get; } = new UniverseSyncCache();
        public long CurrentCacheSize { get; private set; }

        private static string CacheDirectory => CommonUtil.CombinePaths(Client.KspPath, "GameData", "LunaMultiPlayer", "Cache");

        private AutoResetEvent IncomingEvent { get; } = new AutoResetEvent(false);
        private ConcurrentQueue<byte[]> IncomingQueue { get; } = new ConcurrentQueue<byte[]>();
        private Dictionary<string, long> FileLengths { get; } = new Dictionary<string, long>();
        private Dictionary<string, DateTime> FileCreationTimes { get; } = new Dictionary<string, DateTime>();

        #endregion

        #region Public methods

        public string[] GetCachedObjects()
        {
            var cacheFiles = GetCachedFiles();
            var cacheObjects = new string[cacheFiles.Length];
            for (var i = 0; i < cacheFiles.Length; i++)
                cacheObjects[i] = Path.GetFileNameWithoutExtension(cacheFiles[i]);
            return cacheObjects;
        }

        public void ExpireCache()
        {
            Debug.Log("Expiring cache!");
            //No folder, no delete.
            if (!Directory.Exists(CommonUtil.CombinePaths(CacheDirectory, "Incoming")))
            {
                Debug.Log("No sync cache folder, skipping expire.");
                return;
            }
            //Delete partial incoming files
            var incomingFiles = Directory.GetFiles(CommonUtil.CombinePaths(CacheDirectory, "Incoming"));
            foreach (var incomingFile in incomingFiles)
            {
                Debug.Log("Deleting partially cached object " + incomingFile);
                File.Delete(incomingFile);
            }
            //Delete old files
            var cacheObjects = GetCachedObjects();
            CurrentCacheSize = 0;
            foreach (var cacheObject in cacheObjects)
            {
                var cacheFile = CommonUtil.CombinePaths(CacheDirectory, cacheObject + ".txt");
                //If the file is older than a week, delete it.
                if (File.GetCreationTime(cacheFile).AddDays(7d) < DateTime.Now)
                {
                    Debug.Log("Deleting cached object " + cacheObject + ", reason: Expired!");
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
            while (CurrentCacheSize > SettingsSystem.CurrentSettings.CacheSize*1024*1024)
            {
                string deleteObject = null;
                //Find oldest file
                foreach (var testFile in FileCreationTimes)
                {
                    if (deleteObject == null)
                        deleteObject = testFile.Key;
                    if (testFile.Value < FileCreationTimes[deleteObject])
                        deleteObject = testFile.Key;
                }
                Debug.Log("Deleting cached object " + deleteObject + ", reason: Cache full!");
                var deleteFile = CommonUtil.CombinePaths(CacheDirectory, deleteObject + ".txt");
                File.Delete(deleteFile);
                CurrentCacheSize -= FileLengths[deleteObject];
                if (FileCreationTimes.ContainsKey(deleteObject))
                    FileCreationTimes.Remove(deleteObject);
                if (FileLengths.ContainsKey(deleteObject))
                    FileLengths.Remove(deleteObject);
            }
        }

        /// <summary>
        ///     Queues to cache. This method is non-blocking, using SaveToCache for a blocking method.
        /// </summary>
        /// <param name="fileData">File data.</param>
        public void QueueToCache(byte[] fileData)
        {
            IncomingQueue.Enqueue(fileData);
            IncomingEvent.Set();
        }

        public byte[] GetFromCache(string objectName)
        {
            var objectFile = CommonUtil.CombinePaths(CacheDirectory, objectName + ".txt");
            if (File.Exists(objectFile))
                return File.ReadAllBytes(objectFile);
            throw new IOException("Cached object " + objectName + " does not exist");
        }

        public void DeleteCache()
        {
            Debug.Log("Deleting cache!");
            foreach (var cacheFile in GetCachedFiles())
                File.Delete(cacheFile);
            FileLengths.Clear();
            FileCreationTimes.Clear();
            CurrentCacheSize = 0;
        }

        #endregion

        #region Private methods

        private void ProcessingThreadMain()
        {
            while (true)
            {
                byte[] incomingBytes;
                if (IncomingQueue.TryDequeue(out incomingBytes))
                    SaveToCache(incomingBytes);
                else
                    IncomingEvent.WaitOne(500);
            }
        }

        private static string[] GetCachedFiles()
        {
            return Directory.GetFiles(CacheDirectory);
        }

        private void SaveToCache(byte[] fileData)
        {
            if ((fileData == null) || (fileData.Length == 0))
                return;
            var objectName = Common.CalculateSha256Hash(fileData);
            var objectFile = CommonUtil.CombinePaths(CacheDirectory, objectName + ".txt");
            var incomingFile = CommonUtil.CombinePaths(CacheDirectory, "Incoming", objectName + ".txt");
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