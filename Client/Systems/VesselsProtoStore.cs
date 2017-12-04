using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaClient.VesselUtilities;
using LunaCommon;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems
{
    /// <summary>
    /// This class holds a dictionary with all the vessel definitions that we received either when 2 vessels are docked or some vessel part changes or 
    /// a vessel gets a change. It's a way of unifying the data-storage of VesselChangeSystem and VesselProtoSystem and also be able to update a 
    /// vessel definition when a docking happens or to acces this dictionary from the VesselRemoveSystem
    /// </summary>
    public class VesselsProtoStore
    {
        public static ConcurrentDictionary<Guid, VesselProtoUpdate> AllPlayerVessels { get; } =
            new ConcurrentDictionary<Guid, VesselProtoUpdate>();

        /// <summary>
        /// In this method we get the new vessel data and set it to the dictionary of all the player vessels.
        /// </summary>
        public static void HandleVesselProtoData(byte[] vesselData, Guid vesselId, bool runSyncronously = false)
        {
            if (runSyncronously)
                HandleData();
            else
                SystemBase.TaskFactory.StartNew(HandleData);

            void HandleData()
            {
                if (AllPlayerVessels.TryGetValue(vesselId, out var vesselUpdate))
                {
                    vesselUpdate.Update(vesselData, vesselId);
                }
                else
                {
                    AllPlayerVessels.TryAdd(vesselId, new VesselProtoUpdate(vesselData, vesselId));
                }

            }
        }

        /// <summary>
        /// Clears the whole system
        /// </summary>
        public static void ClearSystem()
        {
            AllPlayerVessels.Clear();
        }

        /// <summary>
        /// Removes a vessel from the proto system. Bar in mind that if we receive a protovessel msg after this method is called it will be reloaded
        /// </summary>
        public static void RemoveVessel(Guid vesselId)
        {
            AllPlayerVessels.TryRemove(vesselId, out var _);
        }
    }

    public class VesselProtoUpdate
    {
        public Guid VesselId { get; set; }
        public ConfigNode VesselNode { get; set; }
        public string VesselHash { get; set; }
        public bool UpdatesChecked { get; set; }

        private ProtoVessel _protoVessel;
        public ProtoVessel ProtoVessel
        {
            get => _protoVessel ?? (_protoVessel = VesselCommon.CreateSafeProtoVesselFromConfigNode(VesselNode, VesselId));
            set => _protoVessel = value;
        }

        public Vessel Vessel => FlightGlobals.FindVessel(VesselId);
        public bool VesselExist => Vessel != null;
        public bool ShouldBeLoaded => SettingsSystem.ServerSettings.ShowVesselsInThePast ||
                                      !VesselCommon.VesselIsControlledAndInPastSubspace(VesselId);

        public VesselProtoUpdate(byte[] vesselData, Guid vesselId)
        {
            VesselId = vesselId;
            VesselNode = ConfigNodeSerializer.Deserialize(vesselData);
            VesselHash = Common.CalculateSha256Hash(vesselData);
        }

        public void Update(byte[] vesselData, Guid vesselId)
        {
            if (VesselId != vesselId)
            {
                LunaLog.LogError("Cannot update a VesselProtoUpdate with a differente vesselId");
                return;
            }

            var newHash = Common.CalculateSha256Hash(vesselData);
            if (VesselHash == newHash) return; //Skip Updating as the hash is the same

            VesselNode = ConfigNodeSerializer.Deserialize(vesselData);
            ProtoVessel = VesselCommon.CreateSafeProtoVesselFromConfigNode(VesselNode, VesselId);
            VesselHash = newHash;
            UpdatesChecked = false;
        }
    }
}
