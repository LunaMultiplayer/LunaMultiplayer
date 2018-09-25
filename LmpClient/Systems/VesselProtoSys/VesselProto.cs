using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Utilities;
using LmpClient.VesselUtilities;
using System;

namespace LmpClient.Systems.VesselProtoSys
{
    public class VesselProto
    {
        public Guid VesselId;
        public byte[] RawData = new byte[0];
        public int NumBytes;
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
                VesselRemoveSystem.Singleton.KillVessel(VesselId, "Malformed vessel");
                VesselRemoveSystem.Singleton.AddToKillList(VesselId, "Malformed vessel");
                return null;
            }

            var newProto = VesselSerializer.CreateSafeProtoVesselFromConfigNode(configNode, VesselId);
            if (newProto == null)
            {
                LunaLog.LogError($"Received a malformed vessel from SERVER. Id {VesselId}");
                VesselRemoveSystem.Singleton.KillVessel(VesselId, "Malformed vessel");
                VesselRemoveSystem.Singleton.AddToKillList(VesselId, "Malformed vessel");
                return null;
            }

            if (VesselCommon.ProtoVesselHasInvalidParts(newProto))
                return null;

            return newProto;
        }
    }
}
