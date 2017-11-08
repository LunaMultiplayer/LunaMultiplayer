using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal VesselBaseMsgData() { }
        public override ushort SubType => (ushort)(int)VesselMessageType;

        public virtual VesselMessageType VesselMessageType => throw new NotImplementedException();
    }
}
