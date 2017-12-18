using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.MasterServer
{
    public class MsRegisterServerMsgData : MsBaseMsgData
    {
        /// <inheritdoc />
        internal MsRegisterServerMsgData() { }
        public override MasterServerMessageSubType MasterServerMessageSubType => MasterServerMessageSubType.RegisterServer;

        public long Id;
        public string ServerVersion;
        public string InternalEndpoint;
        public bool Cheats;
        public int GameMode;
        public int MaxPlayers;
        public int ModControl;
        public int PlayerCount;
        public string ServerName;
        public string Description;
        public int WarpMode;
        public int TerrainQuality;
        public int VesselUpdatesSendMsInterval;
        public int SecondaryVesselUpdatesSendMsInterval;
        public bool DropControlOnVesselSwitching;
        public bool DropControlOnExitFlight;
        public bool DropControlOnExit;
        public bool ShowVesselsInThePast;

        public override string ClassName { get; } = nameof(MsRegisterServerMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(Id);
            lidgrenMsg.Write(ServerVersion);
            lidgrenMsg.Write(InternalEndpoint);
            lidgrenMsg.Write(Cheats);
            lidgrenMsg.Write(GameMode);
            lidgrenMsg.Write(MaxPlayers);
            lidgrenMsg.Write(ModControl);
            lidgrenMsg.Write(PlayerCount);
            lidgrenMsg.Write(ServerName);
            lidgrenMsg.Write(Description);
            lidgrenMsg.Write(WarpMode);
            lidgrenMsg.Write(TerrainQuality);
            lidgrenMsg.Write(VesselUpdatesSendMsInterval);
            lidgrenMsg.Write(SecondaryVesselUpdatesSendMsInterval);
            lidgrenMsg.Write(DropControlOnVesselSwitching);
            lidgrenMsg.Write(DropControlOnExitFlight);
            lidgrenMsg.Write(DropControlOnExit);
            lidgrenMsg.Write(ShowVesselsInThePast);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            Id = lidgrenMsg.ReadInt64();
            ServerVersion = lidgrenMsg.ReadString();
            InternalEndpoint = lidgrenMsg.ReadString();
            Cheats = lidgrenMsg.ReadBoolean();
            GameMode = lidgrenMsg.ReadInt32();
            MaxPlayers = lidgrenMsg.ReadInt32();
            ModControl = lidgrenMsg.ReadInt32();
            PlayerCount = lidgrenMsg.ReadInt32();
            ServerName = lidgrenMsg.ReadString();
            Description = lidgrenMsg.ReadString();
            WarpMode = lidgrenMsg.ReadInt32();
            TerrainQuality = lidgrenMsg.ReadInt32();
            VesselUpdatesSendMsInterval = lidgrenMsg.ReadInt32();
            SecondaryVesselUpdatesSendMsInterval = lidgrenMsg.ReadInt32();
            DropControlOnVesselSwitching = lidgrenMsg.ReadBoolean();
            DropControlOnExitFlight = lidgrenMsg.ReadBoolean();
            DropControlOnExit = lidgrenMsg.ReadBoolean();
            ShowVesselsInThePast = lidgrenMsg.ReadBoolean();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + sizeof(long) + ServerVersion.GetByteCount() + InternalEndpoint.GetByteCount() +
                sizeof(bool) * 5 + sizeof(int) * 8 + ServerName.GetByteCount() + Description.GetByteCount();
        }
    }
}
