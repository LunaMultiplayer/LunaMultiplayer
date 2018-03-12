using LunaClient.Base;
using LunaClient.Utilities;
using LunaCommon.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LunaClient.Systems.CraftLibrary
{
    public class CraftLibrarySystem : MessageSystem<CraftLibrarySystem, CraftLibraryMessageSender, CraftLibraryMessageHandler>
    {
        private CraftLibraryEvents CraftLibraryEventHandler { get; } = new CraftLibraryEvents();

        #region Fields

        //Public
        public ConcurrentQueue<CraftChangeEntry> CraftAddQueue { get; private set; } = new ConcurrentQueue<CraftChangeEntry>();
        public ConcurrentQueue<CraftChangeEntry> CraftDeleteQueue { get; private set; } = new ConcurrentQueue<CraftChangeEntry>();
        public ConcurrentQueue<CraftResponseEntry> CraftResponseQueue { get; private set; } = new ConcurrentQueue<CraftResponseEntry>();

        public string SelectedPlayer { get; set; }
        public List<string> PlayersWithCrafts { get; } = new List<string>();
        //Player -> Craft type -> Craft Name
        public Dictionary<string, Dictionary<CraftType, List<string>>> PlayerList { get; } =
            new Dictionary<string, Dictionary<CraftType, List<string>>>();

        //Craft type -> Craft Name
        public Dictionary<CraftType, IEnumerable<string>> UploadList { get; } = new Dictionary<CraftType, IEnumerable<string>>();

        #region Paths

        private static string SavePath { get; } = CommonUtil.CombinePaths(MainSystem.KspPath, "saves", "LunaMultiplayer");
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

        public override string SystemName { get; } = nameof(CraftLibrarySystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            BuildUploadList();
            SetupRoutine(new RoutineDefinition(2500, RoutineExecution.Update, HandleCraftLibraryEvents));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            CraftAddQueue = new ConcurrentQueue<CraftChangeEntry>();
            CraftDeleteQueue = new ConcurrentQueue<CraftChangeEntry>();
            CraftResponseQueue = new ConcurrentQueue<CraftResponseEntry>();
            PlayersWithCrafts.Clear();
            PlayerList.Clear();
            UploadList.Clear();
            SelectedPlayer = "";
            UploadCraftType = CraftType.Vab;
            UploadCraftName = "";
            DownloadCraftType = CraftType.Vab;
            DownloadCraftName = "";
            DeleteCraftType = CraftType.Vab;
            DeleteCraftName = "";
        }

        #endregion

        #region Update methods

        private void HandleCraftLibraryEvents()
        {
            if (Enabled)
            {
                CraftLibraryEventHandler.HandleCraftLibraryEvents();
            }
        }

        #endregion

        #region Public methods

        public void BuildUploadList()
        {
            UploadList.Clear();
            if (Directory.Exists(VabPath))
                UploadList.Add(CraftType.Vab,
                    Directory.GetFiles(VabPath).Select(Path.GetFileNameWithoutExtension));
            if (Directory.Exists(SphPath))
                UploadList.Add(CraftType.Sph,
                    Directory.GetFiles(SphPath).Select(Path.GetFileNameWithoutExtension));
            if (Directory.Exists(VabPath))
                UploadList.Add(CraftType.Subassembly,
                    Directory.GetFiles(SubassemblyPath).Select(Path.GetFileNameWithoutExtension));
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