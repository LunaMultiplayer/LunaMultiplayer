using System;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselUpdateMsgData : VesselBaseMsgData
    {
        public override VesselMessageType VesselMessageType => VesselMessageType.Update;
        public Guid VesselId { get; set; }
        public int Stage { get; set; }
        public uint[] ActiveEngines { get; set; }
        public uint[] StoppedEngines { get; set; }
        public uint[] Decouplers { get; set; }
        public uint[] AnchoredDecouplers { get; set; }
        public uint[] Clamps { get; set; }
        public uint[] Docks { get; set; }
        public bool[] ActiongroupControls { get; set; }
        public uint[] OpenedShieldedDocks { get; set; }
        public uint[] ClosedShieldedDocks { get; set; }
    }
}