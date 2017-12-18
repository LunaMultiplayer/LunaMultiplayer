using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryListReplyMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryListReplyMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.ListReply;

        public int PlayerCraftsCount;
        public PlayerCrafts[] PlayerCrafts = new PlayerCrafts[0];

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(PlayerCraftsCount);
            for (var i = 0; i < PlayerCraftsCount; i++)
            {
                PlayerCrafts[i].Serialize(lidgrenMsg, dataCompressed);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            PlayerCraftsCount = lidgrenMsg.ReadInt32();
            PlayerCrafts = ArrayPool<PlayerCrafts>.Claim(PlayerCraftsCount);

            for (var i = 0; i < PlayerCraftsCount; i++)
            {
                if(PlayerCrafts[i] == null)
                    PlayerCrafts[i] = new PlayerCrafts();

                PlayerCrafts[i].Deserialize(lidgrenMsg, dataCompressed);
            }
        }

        public override void Recycle()
        {
            base.Recycle();

            for (var i = 0; i < PlayerCraftsCount; i++)
            {
                PlayerCrafts[i].Recycle();
            }
            ArrayPool<PlayerCrafts>.Release(ref PlayerCrafts);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            var arraySize = 0;
            for (var i = 0; i < PlayerCraftsCount; i++)
            {
                arraySize += PlayerCrafts[i].GetByteCount();
            }

            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + arraySize;
        }
    }
}