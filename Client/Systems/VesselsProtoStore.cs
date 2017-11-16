using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselChangeSys;
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
                var newProtoUpd = GetVesselProtoUpdateFromBytes(vesselData, vesselId);
                if (newProtoUpd == null) return;

                SystemsContainer.Get<VesselChangeSystem>().ProcessVesselChange(newProtoUpd.ProtoVessel);
                AllPlayerVessels.AddOrUpdate(vesselId, newProtoUpd, (key, existingVal) => newProtoUpd);
            }
        }

        /// <summary>
        /// Returns a VesselProtoUpdate structure from a bytearray and a vesselid.
        /// If there's an error it returns null.
        /// </summary>
        public static VesselProtoUpdate GetVesselProtoUpdateFromBytes(byte[] vesselData, Guid vesselId)
        {
            var vesselNode = ConfigNodeSerializer.Deserialize(vesselData);
            if (vesselNode != null && vesselId == Common.ConvertConfigStringToGuid(vesselNode.GetValue("pid")))
            {
                //TODO: Check if this can be improved as it probably creates a lot of garbage in memory
                return new VesselProtoUpdate(vesselNode, vesselId);
            }

            return null;
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

        /// <summary>
        /// Sets a vessel as unloaded so it can be recreated later. 
        /// For example if you leave a subspace the vessel must still be in the system but it should be unloaded
        /// </summary>
        public static void UnloadVessel(Guid vesselId)
        {
            if (AllPlayerVessels.TryGetValue(vesselId, out var existingProtoUpdate))
            {
                AllPlayerVessels.TryUpdate(vesselId, new VesselProtoUpdate(existingProtoUpdate), existingProtoUpdate);
            }
        }
    }

    public class VesselProtoUpdate
    {
        public Guid VesselId { get; set; }

        private ProtoVessel _protoVessel;
        public ProtoVessel ProtoVessel
        {
            get => _protoVessel ?? (_protoVessel = VesselCommon.CreateSafeProtoVesselFromConfigNode(VesselNode, VesselId));
            set => _protoVessel = value;
        }

        public ConfigNode VesselNode { get; set; }
        public Vessel Vessel => FlightGlobals.FindVessel(VesselId);
        public bool VesselExist => Vessel != null;
        public bool ShouldBeLoaded => SettingsSystem.ServerSettings.ShowVesselsInThePast ||
                                      !VesselCommon.VesselIsControlledAndInPastSubspace(VesselId);

        public VesselProtoUpdate(ConfigNode vessel, Guid vesselId)
        {
            VesselId = vesselId;
            VesselNode = vessel;
        }

        public VesselProtoUpdate(VesselProtoUpdate protoUpdate)
        {
            VesselId = protoUpdate.VesselId;
            ProtoVessel = protoUpdate.ProtoVessel;
        }
    }
}
