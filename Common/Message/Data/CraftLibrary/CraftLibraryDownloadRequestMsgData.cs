using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryDownloadRequestMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryDownloadRequestMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.DownloadRequest;

        public CraftBasicInfo CraftRequested = new CraftBasicInfo();

        public override string ClassName { get; } = nameof(CraftLibraryDownloadRequestMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            CraftRequested.Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            CraftRequested.Deserialize(lidgrenMsg);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + CraftRequested.GetByteCount();
        }
    }
}
