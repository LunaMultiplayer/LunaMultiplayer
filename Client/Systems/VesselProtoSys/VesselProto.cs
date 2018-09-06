using System;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProto
    {
        public Guid VesselId;
        public byte[] RawData;
        public int NumBytes;
        public bool ForceReload;
        public double GameTime;

        public Vessel LoadVessel()
        {
            return null;
        }
    }
}
