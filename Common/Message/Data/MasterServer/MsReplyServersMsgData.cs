using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.MasterServer
{
    public class MsReplyServersMsgData : MsBaseMsgData
    {
        /// <inheritdoc />
        internal MsReplyServersMsgData() { }
        public override MasterServerMessageSubType MasterServerMessageSubType => MasterServerMessageSubType.ReplyServers;

        public long[] Id { get; set; }
        public string[] Ip { get; set; }
        public string[] InternalEndpoint { get; set; }
        public string[] ExternalEndpoint { get; set; }
        public bool[] Cheats { get; set; }
        public int[] GameMode { get; set; }
        public int[] MaxPlayers { get; set; }
        public int[] ModControl { get; set; }
        public int[] PlayerCount { get; set; }
        public string[] ServerName { get; set; }
        public string[] Description { get; set; }
        public int[] WarpMode { get; set; }
        public int[] VesselUpdatesSendMsInterval { get; set; }
        public bool[] DropControlOnVesselSwitching { get; set; }
        public bool[] DropControlOnExitFlight { get; set; }
        public bool[] DropControlOnExit { get; set; }
    }
}
