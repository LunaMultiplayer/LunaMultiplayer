using LunaCommon.Message.Data.Vessel;
using System;

namespace Server.System.VesselRelay
{
    public class VesselRelayItem
    {
        public int SubspaceId { get; set; }
        public double GameTime { get; set; }

        public Guid VesselId { get; }

        public VesselBaseMsgData Msg { get; }

        public VesselRelayItem()
        {

        }

        public VesselRelayItem(int subspaceId, Guid vesselId, double gameTime, VesselBaseMsgData msg)
        {
            SubspaceId = subspaceId;
            VesselId = vesselId;
            GameTime = gameTime;
            Msg = msg;
        }
    }
}
