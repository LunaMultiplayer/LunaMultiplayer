using LunaClient.Systems;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Utilities;
using LunaClient.VesselUtilities;
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
        public bool IsInSafetyBubble => VesselCommon.IsInSafetyBubble(ProtoVessel);

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
        private bool _needToDeserializeData = true;

        #endregion

        #endregion

        public VesselProtoUpdate(byte[] vesselData, int numBytes, Guid vesselId)
        {
            _numBytes = numBytes;

            VesselId = vesselId;
            CopyVesselBytes(vesselData);
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

            _numBytes = numBytes;
            CopyVesselBytes(vesselData);
            
            _needToDeserializeData = true;
            VesselHasUpdate = true;
        }

        /// <summary>
        /// Copies the received vessel bytes to this class
        /// </summary>
        private void CopyVesselBytes(byte[] vesselData)
        {
            if (_vesselData.Length < _numBytes)
                Array.Resize(ref _vesselData, _numBytes);
            Array.Copy(vesselData, _vesselData, _numBytes);
        }

        /// <summary>
        /// This method uses a lot of memory so try to call it as less as possible and only when needed
        /// </summary>
        public void DeserializeVesselBytes()
        {
            _needToDeserializeData = false;

            _vesselNode = ConfigNodeSerializer.Deserialize(_vesselData, _numBytes);

            var newProto = VesselCommon.CreateSafeProtoVesselFromConfigNode(_vesselNode, VesselId);
            
            //In case there's a deserialization error skip it and keep the older proto
            if (newProto != null) 
                ProtoVessel = newProto;

            //If protovessel is still null then unfortunately we must remove that vessel as the server sent us a bad vessel
            if (_protoVessel == null)
            {
                LunaLog.LogError($"Received a malformed vessel from SERVER. Id {VesselId}");
                SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(VesselId);
            }

            VesselParts.Clear();
            foreach (var protoPart in ProtoVessel.protoPartSnapshots)
            {
                VesselParts.TryAdd(protoPart.flightID, protoPart);
            }
        }
    }
}
