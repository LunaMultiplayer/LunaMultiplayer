using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.MasterServer
{
    public class MsRegisterServerMsgData : MsBaseMsgData
    {
        /// <inheritdoc />
        internal MsRegisterServerMsgData() { }
        public override MasterServerMessageSubType MasterServerMessageSubType => MasterServerMessageSubType.RegisterServer;

        public long Id { get; set; }
        public string ServerVersion { get; set; }
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
        public int SecondaryVesselUpdatesSendMsInterval { get; set; }
        public bool DropControlOnVesselSwitching { get; set; }
        public bool DropControlOnExitFlight { get; set; }
        public bool DropControlOnExit { get; set; }
        public bool ShowVesselsInThePast { get; set; }
    }
}
