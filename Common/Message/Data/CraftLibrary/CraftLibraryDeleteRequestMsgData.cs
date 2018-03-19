using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryDeleteRequestMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryDeleteRequestMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.DeleteRequest;

        public CraftBasicInfo CraftToDelete = new CraftBasicInfo();

        public override string ClassName { get; } = nameof(CraftLibraryDownloadRequestMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            CraftToDelete.Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            CraftToDelete.Deserialize(lidgrenMsg);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + CraftToDelete.GetByteCount();
        }
    }
}
