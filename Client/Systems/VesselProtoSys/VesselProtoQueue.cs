using LunaClient.Base;
using LunaCommon.Message.Data.Vessel;
using System;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoQueue : CachedConcurrentQueue<VesselProto, VesselProtoMsgData>
    {
        protected override void AssignFromMessage(VesselProto value, VesselProtoMsgData msgData)
        {
            value.GameTime = msgData.GameTime;
            value.VesselId = msgData.VesselId;
            value.ForceReload = msgData.ForceReload;
            value.NumBytes = msgData.NumBytes;

            if (value.RawData.Length < msgData.NumBytes)
                value.RawData = new byte[msgData.NumBytes];

            Array.Copy(msgData.Data, value.RawData, msgData.NumBytes);
        }
    }
}
