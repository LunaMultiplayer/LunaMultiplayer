using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupListResponseMsgData : GroupBaseMsgData
    {
        /// <inheritdoc />
        internal GroupListResponseMsgData() { }
        public override GroupMessageType GroupMessageType => GroupMessageType.ListResponse;

        public int GroupsCount;
        public Group[] Groups = new Group[0];

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(GroupsCount);
            for (var i = 0; i < GroupsCount; i++)
            {
                Groups[i].Serialize(lidgrenMsg, dataCompressed);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            GroupsCount = lidgrenMsg.ReadInt32();
            Groups = ArrayPool<Group>.Claim(GroupsCount);

            for (var i = 0; i < GroupsCount; i++)
            {
                if (Groups[i] == null)
                    Groups[i] = new Group();

                Groups[i].Deserialize(lidgrenMsg, dataCompressed);
            }
        }

        public override void Recycle()
        {
            base.Recycle();
            
            for (var i = 0; i < GroupsCount; i++)
            {
                Groups[i].Recycle();
            }
            ArrayPool<Group>.Release(ref Groups);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            var arraySize = 0;
            for (var i = 0; i < GroupsCount; i++)
            {
                arraySize += Groups[i].GetByteCount(dataCompressed);
            }

            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + arraySize;
        }
    }
}
