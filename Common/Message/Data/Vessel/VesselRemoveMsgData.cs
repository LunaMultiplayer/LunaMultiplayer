using System;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselRemoveMsgData : VesselBaseMsgData
    {
        public override VesselMessageType VesselMessageType => VesselMessageType.REMOVE;
        /// <summary>
        ///     Set it to 0 to show it to all players
        /// </summary>
        public double PlanetTime { get; set; }

        public Guid VesselId { get; set; }
        public bool IsDockingUpdate { get; set; }

        /// <summary>
        ///     Only set if IsDockingUpdate == true
        /// </summary>
        public string DockingPlayerName { get; set; }
    }
}