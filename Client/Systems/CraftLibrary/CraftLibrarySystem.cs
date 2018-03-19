using LunaClient.Base;
using LunaClient.Utilities;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems.CraftLibrary
{
    public class CraftLibrarySystem : MessageSystem<CraftLibrarySystem, CraftLibraryMessageSender, CraftLibraryMessageHandler>
    {
        #region Fields and properties

        private static readonly string CraftsFolder = CommonUtil.CombinePaths(MainSystem.KspPath, "saves", "LunaMultiplayer");

        private static DateTime _lastuploadedCraft = DateTime.MinValue;

        public ConcurrentDictionary<string, ConcurrentDictionary<string, CraftBasicEntry>> CraftInfo { get; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, CraftBasicEntry>>();
        public ConcurrentDictionary<string, ConcurrentDictionary<string, CraftEntry>> CraftDownloaded { get; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, CraftEntry>>();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(CraftLibrarySystem);

        protected override bool ProcessMessagesInUnityThread => false;
        
        protected override void OnDisabled()
        {
            base.OnDisabled();
            CraftInfo.Clear();
            CraftDownloaded.Clear();
        }

        #endregion

        public void RefreshOwnCrafts()
        {
            throw new NotImplementedException();
        }
    }
}
