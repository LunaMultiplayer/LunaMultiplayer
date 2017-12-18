using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminListReplyMsgData : AdminBaseMsgData
    {        
        /// <inheritdoc />
        internal AdminListReplyMsgData() { }

        public override AdminMessageType AdminMessageType => AdminMessageType.ListReply;

        public int AdminsNum;
        public string[] Admins = new string[0];
        
        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            AdminsNum = lidgrenMsg.ReadInt32();
            Admins = ArrayPool<string>.Claim(AdminsNum);
            for (var i = 0; i < AdminsNum; i++)
            {
                Admins[0] = lidgrenMsg.ReadString();
            }
        }

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(AdminsNum);
            for (var i = 0; i < AdminsNum; i++)
            {
                lidgrenMsg.Write(Admins[i]);
            }
        }

        public override void Recycle()
        {
            base.Recycle();
            ArrayPool<string>.Release(ref Admins);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + Admins.GetByteCount(AdminsNum);
        }
    }
}