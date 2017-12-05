using LMP.Server.Log;
using LMP.Server.Utilities;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace LMP.Server.System
{
    /// <summary>
    ///     This class provides thread safe funcionallity for file and folder work
    /// </summary>
    public class FileHandler
    {
        /// <summary>
        ///     This dictionary is for retrieving the correct lock based on the path of the file/folder
        /// </summary>
        private static readonly ConcurrentDictionary<string, object> LockSemaphore =
            new ConcurrentDictionary<string, object>();

        /// <summary>
        ///     Thread safe method to append text
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="text">Text to insert</param>
        public static void AppendToFile(string path, string text)
        {
            lock (GetLockSemaphore(path))
            {
                try
                {
                    File.AppendAllText(path, text);
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Error writing to file: {path}, Exception: {e}");
                }
            }
        }

        /// <summary>
        ///     Thread safe file overwriting method
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="data">Data to insert</param>
        public static void WriteToFile(string path, byte[] data)
        {
            lock (GetLockSemaphore(path))
            {
                File.WriteAllBytes(path, data);
            }
        }

        /// <summary>
        ///     Thread safe file overwriting method
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="text">Text to insert</param>
        public static void WriteToFile(string path, string text)
        {
            lock (GetLockSemaphore(path))
            {
                File.WriteAllText(path, text);
            }
        }

        /// <summary>
        ///     Thread safe file copying
        /// </summary>
        /// <param name="from">From path</param>
        /// <param name="to">To path</param>
        public static void FileCopy(string from, string to)
        {
            lock (GetLockSemaphore(from))
            {
                lock (GetLockSemaphore(to))
                {
                    File.Copy(from, to);
                }
            }
        }

        /// <summary>
        ///     Thread safe file reading method
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>Bytes of the file</returns>
        public static byte[] ReadFile(string path)
        {
            lock (GetLockSemaphore(path))
            {
                return File.ReadAllBytes(path);
            }
        }

        /// <summary>
        ///     Thread safe file text reading method
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>Bytes of the file</returns>
        public static string ReadFileText(string path)
        {
            lock (GetLockSemaphore(path))
            {
                return File.ReadAllText(path);
            }
        }

        /// <summary>
        ///     Thread safe file text reading method
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>Test lines of the file</returns>
        public static string[] ReadFileLines(string path)
        {
            lock (GetLockSemaphore(path))
            {
                return File.ReadAllLines(path);
            }
        }

        /// <summary>
        ///     Thread safe file exist method
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>File exists or not</returns>
        public static bool FileExists(string path)
        {
            lock (GetLockSemaphore(path))
            {
                return File.Exists(path);
            }
        }

        /// <summary>
        ///     Thread safe folder exist method
        /// </summary>
        /// <param name="path">Path to the folder</param>
        /// <returns>Folder exists or not</returns>
        public static bool FolderExists(string path)
        {
            lock (GetLockSemaphore(path))
            {
                return Directory.Exists(path);
            }
        }

        /// <summary>
        ///     Thread safe folder delete method
        /// </summary>
        /// <param name="path">Path to the folder</param>
        public static void FolderDelete(string path)
        {
            lock (GetLockSemaphore(path))
            {
                Directory.Delete(path);
            }
        }

        /// <summary>
        ///     Thread safe folder create method
        /// </summary>
        /// <param name="path">Path to the folder</param>
        /// <returns>Folder exists or not</returns>
        public static void FolderCreate(string path)
        {
            lock (GetLockSemaphore(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        ///     Thread safe file moving method
        /// </summary>
        /// <param name="sourcePath">Original path</param>
        /// <param name="destPath">Destination path</param>
        public static void MoveFile(string sourcePath, string destPath)
        {
            lock (GetLockSemaphore(sourcePath))
            {
                lock (GetLockSemaphore(destPath))
                {
                    File.Move(sourcePath, destPath);
                }
            }
        }

        /// <summary>
        ///     Thread safe file deleting method
        /// </summary>
        /// <param name="path">Path of the file to remove</param>
        public static void FileDelete(string path)
        {
            lock (GetLockSemaphore(path))
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        /// <summary>
        ///     Thread safe retrieval of files in a given path
        /// </summary>
        /// <param name="path">Path to look into</param>
        /// <param name="searchOption">Search options</param>
        /// <returns>List of files</returns>
        public static string[] GetFilesInPath(string path,
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            lock (GetLockSemaphore(path))
            {
                return Directory.GetFiles(path, "*", searchOption).OrderBy(f => new FileInfo(f).CreationTime).ToArray();
            }
        }

        /// <summary>
        ///     Thread safe retrieval of folders in a given path
        /// </summary>
        /// <param name="path">Path to look into</param>
        /// <returns>List of folders</returns>
        public static string[] GetDirectoriesInPath(string path)
        {
            lock (GetLockSemaphore(path))
            {
                return Directory.GetDirectories(path);
            }
        }

        /// <summary>
        ///     Thread safe attribute setting method
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="attributes">Attributes</param>
        public static void SetAttributes(string path, FileAttributes attributes)
        {
            lock (GetLockSemaphore(path))
            {
                if (File.Exists(path))
                    File.SetAttributes(path, attributes);
            }
        }

        /// <summary>
        ///     Method to retrieve the correct lock based on the path we are editing
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static object GetLockSemaphore(string path)
        {
            var realPath = Path.HasExtension(path) ? Path.GetDirectoryName(path) : path;
            if (!string.IsNullOrEmpty(realPath))
            {
                if (!LockSemaphore.TryGetValue(realPath, out var semaphore))
                {
                    semaphore = new object();
                    LockSemaphore.TryAdd(realPath, semaphore);
                }
                return semaphore;
            }
            throw new HandledException($"Bad folder/file path ({path})");
        }
    }
}