using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.MasterServer
{
    public class MsReplyServersMsgData : MsBaseMsgData
    {
        /// <inheritdoc />
        internal MsReplyServersMsgData() { }
        public override MasterServerMessageSubType MasterServerMessageSubType => MasterServerMessageSubType.ReplyServers;

        public int ServersCount;
        public long[] Id = new long[0];
        public string[] ServerVersion = new string[0];
        public string[] InternalEndpoint = new string[0];
        public string[] ExternalEndpoint = new string[0];
        public bool[] Cheats = new bool[0];
        public int[] GameMode = new int[0];
        public int[] MaxPlayers = new int[0];
        public int[] ModControl = new int[0];
        public int[] PlayerCount = new int[0];
        public string[] ServerName = new string[0];
        public string[] Description = new string[0];
        public int[] WarpMode = new int[0];
        public int[] TerrainQuality = new int[0];
        public int[] VesselUpdatesSendMsInterval = new int[0];
        public bool[] DropControlOnVesselSwitching = new bool[0];
        public bool[] DropControlOnExitFlight = new bool[0];
        public bool[] DropControlOnExit = new bool[0];

        public override string ClassName { get; } = nameof(MsReplyServersMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(ServersCount);
            for (var i = 0; i < ServersCount; i++)
            {
                lidgrenMsg.Write(Id[i]);
                lidgrenMsg.Write(ServerVersion[i]);
                lidgrenMsg.Write(InternalEndpoint[i]);
                lidgrenMsg.Write(ExternalEndpoint[i]);
                lidgrenMsg.Write(Cheats[i]);
                lidgrenMsg.Write(GameMode[i]);
                lidgrenMsg.Write(MaxPlayers[i]);
                lidgrenMsg.Write(ModControl[i]);
                lidgrenMsg.Write(PlayerCount[i]);
                lidgrenMsg.Write(ServerName[i]);
                lidgrenMsg.Write(Description[i]);
                lidgrenMsg.Write(WarpMode[i]);
                lidgrenMsg.Write(TerrainQuality[i]);
                lidgrenMsg.Write(VesselUpdatesSendMsInterval[i]);
                lidgrenMsg.Write(DropControlOnVesselSwitching[i]);
                lidgrenMsg.Write(DropControlOnExitFlight[i]);
                lidgrenMsg.Write(DropControlOnExit[i]);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            ServersCount = lidgrenMsg.ReadInt32();

            if (Id.Length < ServersCount)
                Id = new long[ServersCount];
            if (ServerVersion.Length < ServersCount)
                ServerVersion = new string[ServersCount];
            if (InternalEndpoint.Length < ServersCount)
                InternalEndpoint = new string[ServersCount];
            if (ExternalEndpoint.Length < ServersCount)
                ExternalEndpoint = new string[ServersCount];
            if (Cheats.Length < ServersCount)
                Cheats = new bool[ServersCount];
            if (GameMode.Length < ServersCount)
                GameMode = new int[ServersCount];
            if (MaxPlayers.Length < ServersCount)
                MaxPlayers = new int[ServersCount];
            if (ModControl.Length < ServersCount)
                ModControl = new int[ServersCount];
            if (PlayerCount.Length < ServersCount)
                PlayerCount = new int[ServersCount];
            if (ServerName.Length < ServersCount)
                ServerName = new string[ServersCount];
            if (Description.Length < ServersCount)
                Description = new string[ServersCount];
            if (WarpMode.Length < ServersCount)
                WarpMode = new int[ServersCount];
            if (TerrainQuality.Length < ServersCount)
                TerrainQuality = new int[ServersCount];
            if (VesselUpdatesSendMsInterval.Length < ServersCount)
                VesselUpdatesSendMsInterval = new int[ServersCount];
            if (DropControlOnVesselSwitching.Length < ServersCount)
                DropControlOnVesselSwitching = new bool[ServersCount];
            if (DropControlOnExitFlight.Length < ServersCount)
                DropControlOnExitFlight = new bool[ServersCount];
            if (DropControlOnExit.Length < ServersCount)
                DropControlOnExit = new bool[ServersCount];

            for (var i = 0; i < ServersCount; i++)
            {
                Id[i] = lidgrenMsg.ReadInt64();
                ServerVersion[i] = lidgrenMsg.ReadString();
                InternalEndpoint[i] = lidgrenMsg.ReadString();
                ExternalEndpoint[i] = lidgrenMsg.ReadString();
                Cheats[i] = lidgrenMsg.ReadBoolean();
                GameMode[i] = lidgrenMsg.ReadInt32();
                MaxPlayers[i] = lidgrenMsg.ReadInt32();
                ModControl[i] = lidgrenMsg.ReadInt32();
                PlayerCount[i] = lidgrenMsg.ReadInt32();
                ServerName[i] = lidgrenMsg.ReadString();
                Description[i] = lidgrenMsg.ReadString();
                WarpMode[i] = lidgrenMsg.ReadInt32();
                TerrainQuality[i] = lidgrenMsg.ReadInt32();
                VesselUpdatesSendMsInterval[i] = lidgrenMsg.ReadInt32();
                DropControlOnVesselSwitching[i] = lidgrenMsg.ReadBoolean();
                DropControlOnExitFlight[i] = lidgrenMsg.ReadBoolean();
                DropControlOnExit[i] = lidgrenMsg.ReadBoolean();
            }
        }
        
        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + 
                sizeof(long) * ServersCount + ServerVersion.GetByteCount(ServersCount) + InternalEndpoint.GetByteCount(ServersCount) +
                ExternalEndpoint.GetByteCount(ServersCount) + ServerName.GetByteCount(ServersCount) + Description.GetByteCount(ServersCount) +
                sizeof(bool) * 4 * ServersCount + sizeof(int) * 8 * ServersCount;
        }
    }
}
