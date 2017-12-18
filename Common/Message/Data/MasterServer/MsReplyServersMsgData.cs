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

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(ServersCount);
            for (var i = 0; i < ServersCount; i++)
            {
                lidgrenMsg.Write(Id[i]);
                lidgrenMsg.Write(ServerVersion[i]);
                lidgrenMsg.Write(InternalEndpoint[i]);
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

            Id = ArrayPool<long>.Claim(ServersCount);
            ServerVersion = ArrayPool<string>.Claim(ServersCount);
            InternalEndpoint = ArrayPool<string>.Claim(ServersCount);
            Cheats = ArrayPool<bool>.Claim(ServersCount);
            GameMode = ArrayPool<int>.Claim(ServersCount);
            MaxPlayers = ArrayPool<int>.Claim(ServersCount);
            ModControl = ArrayPool<int>.Claim(ServersCount);
            PlayerCount = ArrayPool<int>.Claim(ServersCount);
            ServerName = ArrayPool<string>.Claim(ServersCount);
            Description = ArrayPool<string>.Claim(ServersCount);
            WarpMode = ArrayPool<int>.Claim(ServersCount);
            TerrainQuality = ArrayPool<int>.Claim(ServersCount);
            VesselUpdatesSendMsInterval = ArrayPool<int>.Claim(ServersCount);
            DropControlOnVesselSwitching = ArrayPool<bool>.Claim(ServersCount);
            DropControlOnExitFlight = ArrayPool<bool>.Claim(ServersCount);
            DropControlOnExit = ArrayPool<bool>.Claim(ServersCount);

            for (var i = 0; i < ServersCount; i++)
            {
                Id[i] = lidgrenMsg.ReadInt64();
                ServerVersion[i] = lidgrenMsg.ReadString();
                InternalEndpoint[i] = lidgrenMsg.ReadString();
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

        public override void Recycle()
        {
            base.Recycle();

            ArrayPool<long>.Release(ref Id);
            ArrayPool<string>.Release(ref ServerVersion);
            ArrayPool<string>.Release(ref InternalEndpoint);
            ArrayPool<bool>.Release(ref Cheats);
            ArrayPool<int>.Release(ref GameMode);
            ArrayPool<int>.Release(ref MaxPlayers);
            ArrayPool<int>.Release(ref ModControl);
            ArrayPool<int>.Release(ref PlayerCount);
            ArrayPool<string>.Release(ref ServerName);
            ArrayPool<string>.Release(ref Description);
            ArrayPool<int>.Release(ref WarpMode);
            ArrayPool<int>.Release(ref TerrainQuality);
            ArrayPool<int>.Release(ref VesselUpdatesSendMsInterval);
            ArrayPool<bool>.Release(ref DropControlOnVesselSwitching);
            ArrayPool<bool>.Release(ref DropControlOnExitFlight);
            ArrayPool<bool>.Release(ref DropControlOnExit);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + 
                sizeof(long) * ServersCount + ServerVersion.GetByteCount(ServersCount) + InternalEndpoint.GetByteCount(ServersCount) +
                ExternalEndpoint.GetByteCount(ServersCount) + ServerName.GetByteCount(ServersCount) + Description.GetByteCount(ServersCount) +
                sizeof(bool) * 4 * ServersCount + sizeof(int) * 7 * ServersCount;
        }
    }
}
