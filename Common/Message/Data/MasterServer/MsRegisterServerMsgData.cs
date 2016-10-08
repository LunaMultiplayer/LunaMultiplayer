using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.MasterServer
{
    public class MsRegisterServerMsgData : MsBaseMsgData
    {
        public override MasterServerMessageSubType MasterServerMessageSubType => MasterServerMessageSubType.REGISTER_SERVER;

        public long Id { get; set; }
        public string InternalEndpoint { get; set; }
        public bool Cheats { get; set; }
        public int GameMode { get; set; }
        public int MaxPlayers { get; set; }
        public int ModControl { get; set; }
        public int PlayerCount { get; set; }
        public string ServerName { get; set; }
        public string Description { get; set; }
        public int WarpMode { get; set; }
        public int VesselUpdatesSendMsInterval { get; set; }
        public bool DropControlOnVesselSwitching { get; set; }
        public bool DropControlOnExitFlight { get; set; }
        public bool DropControlOnExit { get; set; }
    }
}
