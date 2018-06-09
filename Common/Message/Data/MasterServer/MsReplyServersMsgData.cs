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

        public long Id;
        public string ServerVersion;
        public string InternalEndpoint;
        public string ExternalEndpoint;
        public bool Password;
        public bool Cheats;
        public bool ModControl;
        public int GameMode;
        public int MaxPlayers;
        public int PlayerCount;
        public string ServerName;
        public string Description;
        public string Country;
        public string Website;
        public string WebsiteText;
        public int WarpMode;
        public int TerrainQuality;
        public int VesselUpdatesSendMsInterval;
        public bool DropControlOnVesselSwitching;
        public bool DropControlOnExitFlight;
        public bool DropControlOnExit;

        public override string ClassName { get; } = nameof(MsReplyServersMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(Id);
            lidgrenMsg.Write(ServerVersion);
            lidgrenMsg.Write(InternalEndpoint);
            lidgrenMsg.Write(ExternalEndpoint);
            lidgrenMsg.Write(Password);
            lidgrenMsg.Write(Cheats);
            lidgrenMsg.Write(ModControl);
            lidgrenMsg.Write(GameMode);
            lidgrenMsg.Write(MaxPlayers);
            lidgrenMsg.Write(PlayerCount);
            lidgrenMsg.Write(ServerName);
            lidgrenMsg.Write(Description);
            lidgrenMsg.Write(Country);
            lidgrenMsg.Write(Website);
            lidgrenMsg.Write(WebsiteText);
            lidgrenMsg.Write(WarpMode);
            lidgrenMsg.Write(TerrainQuality);
            lidgrenMsg.Write(VesselUpdatesSendMsInterval);
            lidgrenMsg.Write(DropControlOnVesselSwitching);
            lidgrenMsg.Write(DropControlOnExitFlight);
            lidgrenMsg.Write(DropControlOnExit);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            Id = lidgrenMsg.ReadInt64();
            ServerVersion = lidgrenMsg.ReadString();
            InternalEndpoint = lidgrenMsg.ReadString();
            ExternalEndpoint = lidgrenMsg.ReadString();
            Password = lidgrenMsg.ReadBoolean();
            Cheats = lidgrenMsg.ReadBoolean();
            ModControl = lidgrenMsg.ReadBoolean();
            GameMode = lidgrenMsg.ReadInt32();
            MaxPlayers = lidgrenMsg.ReadInt32();
            PlayerCount = lidgrenMsg.ReadInt32();
            ServerName = lidgrenMsg.ReadString();
            Description = lidgrenMsg.ReadString();
            Country = lidgrenMsg.ReadString();
            Website = lidgrenMsg.ReadString();
            WebsiteText = lidgrenMsg.ReadString();
            WarpMode = lidgrenMsg.ReadInt32();
            TerrainQuality = lidgrenMsg.ReadInt32();
            VesselUpdatesSendMsInterval = lidgrenMsg.ReadInt32();
            DropControlOnVesselSwitching = lidgrenMsg.ReadBoolean();
            DropControlOnExitFlight = lidgrenMsg.ReadBoolean();
            DropControlOnExit = lidgrenMsg.ReadBoolean();
        }
        
        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + 
                sizeof(long) + ServerVersion.GetByteCount() + InternalEndpoint.GetByteCount() +
                ExternalEndpoint.GetByteCount() + ServerName.GetByteCount() + Description.GetByteCount() + Country .GetByteCount() + Website.GetByteCount() + WebsiteText.GetByteCount() +
                sizeof(bool) * 6 + sizeof(int) * 6;
        }
    }
}
