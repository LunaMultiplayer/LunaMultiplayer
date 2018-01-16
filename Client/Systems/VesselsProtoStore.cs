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
        public static void HandleVesselProtoData(byte[] vesselData, int numBytes, Guid vesselId, bool runSyncronously = false)
        {
            if (runSyncronously)
                HandleData();
            else
                SystemBase.TaskFactory.StartNew(HandleData);

            void HandleData()
            {
                if (AllPlayerVessels.TryGetValue(vesselId, out var vesselUpdate))
                {
                    vesselUpdate.Update(vesselData, numBytes, vesselId);
                }
                else
                {
                    AllPlayerVessels.TryAdd(vesselId, new VesselProtoUpdate(vesselData, numBytes, vesselId));
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
        #region Fields & properties

        public Guid VesselId { get; set; }
        public bool VesselHasUpdate { get; set; }

        private ProtoVessel _protoVessel;
        public ProtoVessel ProtoVessel
        {
            get
            {
                if (VesselHasUpdate || _protoVessel == null)
                {
                    DeserializeVesselBytes();
                    _protoVessel = VesselCommon.CreateSafeProtoVesselFromConfigNode(_vesselNode, VesselId);
                }
                return _protoVessel;
            }
            set => _protoVessel = value;
        }

        public Vessel Vessel => FlightGlobals.FindVessel(VesselId);
        public bool VesselExist => Vessel != null;
        public bool ShouldBeLoaded => SettingsSystem.ServerSettings.ShowVesselsInThePast ||
                                      !VesselCommon.VesselIsControlledAndInPastSubspace(VesselId);

        #region Private

        private byte[] _vesselData = new byte[1000];
        private int _numBytes;
        private ConfigNode _vesselNode;
        private string _vesselHash;

        #endregion

        #endregion

        public VesselProtoUpdate(byte[] vesselData, int numBytes, Guid vesselId)
        {
            VesselId = vesselId;
            CopyVesselBytes(vesselData, numBytes);
        }

        /// <summary>
        /// Update this class with the new data received
        /// </summary>
        public void Update(byte[] vesselData, int numBytes, Guid vesselId)
        {
            if (VesselId != vesselId)
            {
                LunaLog.LogError("Cannot update a VesselProtoUpdate with a differente vesselId");
                return;
            }
            var newHash = Common.CalculateSha256Hash(vesselData);
            if (_vesselHash == newHash) return; //Skip Updating as the hash is the same
            
            CopyVesselBytes(vesselData, numBytes);

            _vesselHash = newHash;
            VesselHasUpdate = true;
        }

        /// <summary>
        /// Copies the received vessel bytes to this class
        /// </summary>
        private void CopyVesselBytes(byte[] vesselData, int numBytes)
        {
            if (_vesselData.Length < numBytes)
                Array.Resize(ref _vesselData, numBytes);
            Array.Copy(vesselData, _vesselData, numBytes);
            _numBytes = numBytes;
        }

        /// <summary>
        /// This method uses a lot of memory so try to call it as less as possible and only when needed
        /// </summary>
        public void DeserializeVesselBytes()
        {
            _vesselNode = ConfigNodeSerializer.Deserialize(_vesselData, _numBytes);
            ProtoVessel = VesselCommon.CreateSafeProtoVesselFromConfigNode(_vesselNode, VesselId);
        }
    }
}
