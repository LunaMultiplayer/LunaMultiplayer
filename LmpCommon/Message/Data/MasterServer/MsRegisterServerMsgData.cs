using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System.Net;

namespace LmpCommon.Message.Data.MasterServer
{
    public class MsRegisterServerMsgData : MsBaseMsgData
    {
        /// <inheritdoc />
        internal MsRegisterServerMsgData() { }
        public override MasterServerMessageSubType MasterServerMessageSubType => MasterServerMessageSubType.RegisterServer;

        public long Id;
        public string ServerVersion;
        public IPEndPoint InternalEndpoint;
        public IPEndPoint InternalEndpoint6;
        public bool Password;
        public bool Cheats;
        public bool ModControl;
        public int GameMode;
        public int MaxPlayers;
        public int PlayerCount;
        public string ServerName;
        public string Description;
        public string CountryCode;
        public string Website;
        public string WebsiteText;
        public int WarpMode;
        public int TerrainQuality;
        public int VesselPositionUpdatesMsInterval;
        public int SecondaryVesselPositionUpdatesMsInterval;
        public bool RainbowEffect;
        public byte[] Color = new byte[3];

        public override string ClassName { get; } = nameof(MsRegisterServerMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(Id);
            lidgrenMsg.Write(ServerVersion);
            lidgrenMsg.Write(InternalEndpoint);
            lidgrenMsg.Write(InternalEndpoint6);

            lidgrenMsg.Write(Password);
            lidgrenMsg.Write(Cheats);
            lidgrenMsg.Write(ModControl);
            lidgrenMsg.WritePadBits();

            lidgrenMsg.Write(GameMode);
            lidgrenMsg.Write(MaxPlayers);
            lidgrenMsg.Write(PlayerCount);
            lidgrenMsg.Write(ServerName);
            lidgrenMsg.Write(Description);
            lidgrenMsg.Write(CountryCode);
            lidgrenMsg.Write(Website);
            lidgrenMsg.Write(WebsiteText);
            lidgrenMsg.Write(WarpMode);
            lidgrenMsg.Write(TerrainQuality);
            lidgrenMsg.Write(VesselPositionUpdatesMsInterval);
            lidgrenMsg.Write(SecondaryVesselPositionUpdatesMsInterval);

            lidgrenMsg.Write(RainbowEffect);
            for (var i = 0; i < 3; i++)
                lidgrenMsg.Write(Color[i]);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            Id = lidgrenMsg.ReadInt64();
            ServerVersion = lidgrenMsg.ReadString();
            InternalEndpoint = lidgrenMsg.ReadIPEndPoint();
            // ReadIPEndPoint supports IPv6 addresses despite saying otherwise in the code doc.
            InternalEndpoint6 = lidgrenMsg.ReadIPEndPoint();

            Password = lidgrenMsg.ReadBoolean();
            Cheats = lidgrenMsg.ReadBoolean();
            ModControl = lidgrenMsg.ReadBoolean();
            lidgrenMsg.SkipPadBits();

            GameMode = lidgrenMsg.ReadInt32();
            MaxPlayers = lidgrenMsg.ReadInt32();
            PlayerCount = lidgrenMsg.ReadInt32();
            ServerName = lidgrenMsg.ReadString();
            Description = lidgrenMsg.ReadString();
            CountryCode = lidgrenMsg.ReadString();
            Website = lidgrenMsg.ReadString();
            WebsiteText = lidgrenMsg.ReadString();
            WarpMode = lidgrenMsg.ReadInt32();
            TerrainQuality = lidgrenMsg.ReadInt32();
            VesselPositionUpdatesMsInterval = lidgrenMsg.ReadInt32();
            SecondaryVesselPositionUpdatesMsInterval = lidgrenMsg.ReadInt32();

            RainbowEffect = lidgrenMsg.ReadBoolean();
            for (var i = 0; i < 3; i++)
                Color[i] = lidgrenMsg.ReadByte();
        }

        internal override int InternalGetMessageSize()
        {
            //We use sizeof(byte) instead of sizeof(bool) because we use the WritePadBits()
            return base.InternalGetMessageSize() + sizeof(long) + ServerVersion.GetByteCount() +
                   InternalEndpoint.GetByteCount() + InternalEndpoint6.GetByteCount() + sizeof(byte) +
                   sizeof(int) * 7 + ServerName.GetByteCount() + Description.GetByteCount() +
                   CountryCode.GetByteCount() + Website.GetByteCount() + WebsiteText.GetByteCount() + sizeof(bool) * 3;
        }
    }
}
