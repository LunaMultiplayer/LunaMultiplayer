using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaClient.VesselUtilities;
using LunaCommon;
using System;
using System.Collections.Concurrent;

namespace LunaClient.VesselStore
{
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
                if (_needToDeserializeData || _protoVessel == null)
                {
                    DeserializeVesselBytes();
                }
                return _protoVessel;
            }
            private set => _protoVessel = value;
        }

        public Vessel Vessel => FlightGlobals.FindVessel(VesselId);
        public bool VesselExist => Vessel != null;
        public bool ShouldBeLoaded => SettingsSystem.ServerSettings.ShowVesselsInThePast || !VesselCommon.VesselIsControlledAndInPastSubspace(VesselId);

        private readonly ConcurrentDictionary<uint, ProtoPartSnapshot> _vesselParts = new ConcurrentDictionary<uint, ProtoPartSnapshot>();
        public ConcurrentDictionary<uint, ProtoPartSnapshot> VesselParts
        {
            get
            {
                if (_needToDeserializeData)
                {
                    DeserializeVesselBytes();
                }
                return _vesselParts;
            }
        }

        #region Private

        private byte[] _vesselData = new byte[1000];
        private int _numBytes;
        private ConfigNode _vesselNode;
        private string _vesselHash;
        private bool _needToDeserializeData = true;

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
        public void Update(byte[] vesselData, int numBytes, Guid vesselId, int situation)
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
            _needToDeserializeData = true;
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
            _needToDeserializeData = false;

            _vesselNode = ConfigNodeSerializer.Deserialize(_vesselData, _numBytes);
            ProtoVessel = VesselCommon.CreateSafeProtoVesselFromConfigNode(_vesselNode, VesselId);

            VesselParts.Clear();
            foreach (var protoPart in ProtoVessel.protoPartSnapshots)
            {
                VesselParts.TryAdd(protoPart.flightID, protoPart);
            }
        }
    }
}
