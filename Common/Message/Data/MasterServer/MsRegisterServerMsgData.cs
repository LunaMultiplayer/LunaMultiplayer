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
        public bool Password;
        public bool Cheats;
        public bool ModControl;
        public int GameMode;
        public int MaxPlayers;
        public int PlayerCount;
        public string ServerName;
        public string Description;
        public string Website;
        public int WarpMode;
        public int TerrainQuality;
        public int VesselPositionUpdatesMsInterval;
        public int SecondaryVesselPositionUpdatesMsInterval;
        public bool DropControlOnVesselSwitching;
        public bool DropControlOnExitFlight;
        public bool DropControlOnExit;
        public bool ShowVesselsInThePast;

        public override string ClassName { get; } = nameof(MsRegisterServerMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(Id);
            lidgrenMsg.Write(ServerVersion);
            lidgrenMsg.Write(InternalEndpoint);

            lidgrenMsg.Write(Password);
            lidgrenMsg.Write(Cheats);
            lidgrenMsg.Write(ModControl);
            lidgrenMsg.WritePadBits();

            lidgrenMsg.Write(GameMode);
            lidgrenMsg.Write(MaxPlayers);
            lidgrenMsg.Write(PlayerCount);
            lidgrenMsg.Write(ServerName);
            lidgrenMsg.Write(Description);
            lidgrenMsg.Write(Website);
            lidgrenMsg.Write(WarpMode);
            lidgrenMsg.Write(TerrainQuality);
            lidgrenMsg.Write(VesselPositionUpdatesMsInterval);
            lidgrenMsg.Write(SecondaryVesselPositionUpdatesMsInterval);

            //4 bits = 1 byte, no need to pad bits here...
            lidgrenMsg.Write(DropControlOnVesselSwitching);
            lidgrenMsg.Write(DropControlOnExitFlight);
            lidgrenMsg.Write(DropControlOnExit);
            lidgrenMsg.Write(ShowVesselsInThePast);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            Id = lidgrenMsg.ReadInt64();
            ServerVersion = lidgrenMsg.ReadString();
            InternalEndpoint = lidgrenMsg.ReadString();

            Password = lidgrenMsg.ReadBoolean();
            Cheats = lidgrenMsg.ReadBoolean();
            ModControl = lidgrenMsg.ReadBoolean();
            lidgrenMsg.SkipPadBits();

            GameMode = lidgrenMsg.ReadInt32();
            MaxPlayers = lidgrenMsg.ReadInt32();
            PlayerCount = lidgrenMsg.ReadInt32();
            ServerName = lidgrenMsg.ReadString();
            Description = lidgrenMsg.ReadString();
            Website = lidgrenMsg.ReadString();
            WarpMode = lidgrenMsg.ReadInt32();
            TerrainQuality = lidgrenMsg.ReadInt32();
            VesselPositionUpdatesMsInterval = lidgrenMsg.ReadInt32();
            SecondaryVesselPositionUpdatesMsInterval = lidgrenMsg.ReadInt32();
            DropControlOnVesselSwitching = lidgrenMsg.ReadBoolean();
            DropControlOnExitFlight = lidgrenMsg.ReadBoolean();
            DropControlOnExit = lidgrenMsg.ReadBoolean();
            ShowVesselsInThePast = lidgrenMsg.ReadBoolean();
        }

        internal override int InternalGetMessageSize()
        {            
            //We use sizeof(byte) instead of sizeof(bool) because we use the WritePadBits()
            return base.InternalGetMessageSize() + sizeof(long) + ServerVersion.GetByteCount() + InternalEndpoint.GetByteCount() +
                sizeof(byte) + sizeof(int) * 7 + ServerName.GetByteCount() + Description.GetByteCount() + Website.GetByteCount() + sizeof(bool) * 4;
        }
    }
}
