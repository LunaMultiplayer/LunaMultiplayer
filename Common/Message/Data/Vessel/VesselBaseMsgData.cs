using System;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)VesselMessageType;

        public virtual VesselMessageType VesselMessageType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
