using Server.Log;
using Server.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server.Command.Command.Base
{
    /// <summary>
    /// This class is made up of threads as it can be called at several times
    /// </summary>
    public abstract class HandledCommand : SimpleCommand
    {
        protected abstract object CommandLock { get; }

        protected abstract string FileName { get; }

        protected virtual List<string> Items { get; set; } = new List<string>();

        protected string FullPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);

        #region Protected methods

        protected HandledCommand() => Load();

        protected void Load()
        {
            //Lists are not thread safe so lock it
            lock (CommandLock)
            {
                //Awaited as we need to have the data before saving
                LoadFromFileIntoList(FullPath, Items);
            }
            Save();
        }

        #endregion

        #region Public methods

        public IEnumerable<string> Retrieve()
        {
            //Lists are not thread safe so lock it
            lock (CommandLock)
            {
                //Awaited
                return Items.ToArray();
            }
        }

        public void Add(string item)
        {
            //Lists are not thread safe so lock it
            lock (CommandLock)
            {
                if (!Items.Contains(item))
                {
                    Items.Add(item);
                    Save();
                }
            }
        }

        public void Remove(string item)
        {
            //Lists are not thread safe so lock it
            lock (CommandLock)
            {
                if (Items.Contains(item))
                {
                    Items.Remove(item);
                    Save();
                }
            }
        }

        public bool Exists(string item)
        {
            //Lists are not thread safe so lock it
            lock (CommandLock)
            {
                return Items.Contains(item);
            }
        }

        #endregion

        #region Private methods

        private void Save()
        {
            //Lists are not thread safe so lock it
            lock (CommandLock)
            {
                try
                {
                    if (FileHandler.FileExists(FullPath))
                    {
                        FileHandler.SetAttributes(FullPath, FileAttributes.Normal);

                        var newItems = Items.Except(FileHandler.ReadFileLines(FullPath)
                                .Select(l => l.Trim())
                                .Where(l => !string.IsNullOrEmpty(l)))
                            .ToList();

                        newItems.ForEach(u => FileHandler.AppendToFile(FullPath, u + Environment.NewLine));
                    }
                    else
                    {
                        Items.ForEach(u => FileHandler.AppendToFile(FullPath, u + Environment.NewLine));
                    }
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Error saving!, Exception: {e}");
                }
            }
        }

        private void LoadFromFileIntoList(string path, ICollection<string> list)
        {
            //Lists are not thread safe so lock it
            lock (CommandLock)
            {
                if (FileHandler.FileExists(path))
                    FileHandler.ReadFileLines(path)
                        .Where(i => !list.Contains(i)).ToList()
                        .ForEach(list.Add);
            }
        }

        #endregion
    }
}
