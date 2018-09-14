using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Utilities;
using LunaClient.VesselUtilities;
using System;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProto
    {
        public Guid VesselId;
        public uint VesselPersistentId;
        public byte[] RawData = new byte[0];
        public int NumBytes;
        public bool ForceReload;
        public double GameTime;

        public Vessel LoadVessel()
        {
            return null;
        }

        public ProtoVessel CreateProtoVessel()
        {
            var configNode = ConfigNodeSerializer.Deserialize(RawData, NumBytes);
            if (configNode == null || VesselCommon.VesselHasNaNPosition(configNode))
            {
                LunaLog.LogError($"Received a malformed vessel from SERVER. Id {VesselId}");
                VesselRemoveSystem.Singleton.KillVessel(VesselPersistentId, VesselId, "Malformed vessel");
                VesselRemoveSystem.Singleton.AddToKillList(VesselPersistentId, VesselId, "Malformed vessel");
                return null;
            }

            var newProto = VesselSerializer.CreateSafeProtoVesselFromConfigNode(configNode, VesselId);
            if (newProto == null)
            {
                LunaLog.LogError($"Received a malformed vessel from SERVER. Id {VesselId}");
                VesselRemoveSystem.Singleton.KillVessel(VesselPersistentId, VesselId, "Malformed vessel");
                VesselRemoveSystem.Singleton.AddToKillList(VesselPersistentId, VesselId, "Malformed vessel");
                return null;
            }

            if (VesselCommon.ProtoVesselHasInvalidParts(newProto))
                return null;

            return newProto;
        }
    }
}
