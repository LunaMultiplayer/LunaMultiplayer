using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LunaClient.Base;
using LunaClient.Utilities;
using LunaCommon.Enums;

namespace LunaClient.Systems.CraftLibrary
{
    public class CraftLibrarySystem :
        MessageSystem<CraftLibrarySystem, CraftLibraryMessageSender, CraftLibraryMessageHandler>
    {
        private CraftLibraryEvents CraftLibraryEventHandler { get; } = new CraftLibraryEvents();

        #region Fields

        //Public
        public Queue<CraftChangeEntry> CraftAddQueue { get; } = new Queue<CraftChangeEntry>();
        public Queue<CraftChangeEntry> CraftDeleteQueue { get; } = new Queue<CraftChangeEntry>();
        public Queue<CraftResponseEntry> CraftResponseQueue { get; } = new Queue<CraftResponseEntry>();

        public string SelectedPlayer { get; set; }
        public List<string> PlayersWithCrafts { get; } = new List<string>();
        //Player -> Craft type -> Craft Name
        public Dictionary<string, Dictionary<CraftType, List<string>>> PlayerList { get; } =
            new Dictionary<string, Dictionary<CraftType, List<string>>>();

        //Craft type -> Craft Name
        public Dictionary<CraftType, List<string>> UploadList { get; } = new Dictionary<CraftType, List<string>>();

        #region Paths

        private static string SavePath { get; } = CommonUtil.CombinePaths(Client.KspPath, "saves", "LunaMultiPlayer");
        public string VabPath { get; } = CommonUtil.CombinePaths(SavePath, "Ships", "VAB");
        public string SphPath { get; } = CommonUtil.CombinePaths(SavePath, "Ships", "SPH");
        public string SubassemblyPath { get; } = CommonUtil.CombinePaths(SavePath, "Subassemblies");

        #endregion

        //upload event
        public CraftType UploadCraftType { get; set; }
        public string UploadCraftName { get; set; }
        //download event
        public CraftType DownloadCraftType { get; set; }
        public string DownloadCraftName { get; set; }
        //delete event
        public CraftType DeleteCraftType { get; set; }
        public string DeleteCraftName { get; set; }

        #endregion

        #region Base overrides

        public override void Update()
        {
            base.Update();
            if (Enabled && MainSystem.Singleton.GameRunning)
            {
                CraftLibraryEventHandler.HandleCraftLibraryEvents();
            }
        }

        public override void OnEnabled()
        {
            base.OnEnabled();
            BuildUploadList();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            CraftAddQueue.Clear();
            CraftDeleteQueue.Clear();
            CraftResponseQueue.Clear();
            PlayersWithCrafts.Clear();
            PlayerList.Clear();
            UploadList.Clear();
            SelectedPlayer = "";
            UploadCraftType = CraftType.VAB;
            UploadCraftName = "";
            DownloadCraftType = CraftType.VAB;
            DownloadCraftName = "";
            DeleteCraftType = CraftType.VAB;
            DeleteCraftName = "";
        }

        #endregion

        #region Public methods

        public void BuildUploadList()
        {
            UploadList.Clear();
            if (Directory.Exists(VabPath))
                UploadList.Add(CraftType.VAB,
                    Directory.GetFiles(VabPath).Select(Path.GetFileNameWithoutExtension).ToList());
            if (Directory.Exists(SphPath))
                UploadList.Add(CraftType.SPH,
                    Directory.GetFiles(SphPath).Select(Path.GetFileNameWithoutExtension).ToList());
            if (Directory.Exists(VabPath))
                UploadList.Add(CraftType.SUBASSEMBLY,
                    Directory.GetFiles(SubassemblyPath).Select(Path.GetFileNameWithoutExtension).ToList());
        }

        public void QueueCraftAdd(CraftChangeEntry entry)
        {
            CraftAddQueue.Enqueue(entry);
        }

        public void QueueCraftDelete(CraftChangeEntry entry)
        {
            CraftDeleteQueue.Enqueue(entry);
        }

        public void QueueCraftResponse(CraftResponseEntry entry)
        {
            CraftResponseQueue.Enqueue(entry);
        }

        #endregion
    }
}