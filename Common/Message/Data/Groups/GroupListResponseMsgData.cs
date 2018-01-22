using Lidgren.Network;
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

        public override string ClassName { get; } = nameof(GroupListResponseMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool compressData)
        {
            base.InternalSerialize(lidgrenMsg, compressData);

            lidgrenMsg.Write(GroupsCount);
            for (var i = 0; i < GroupsCount; i++)
            {
                Groups[i].Serialize(lidgrenMsg, compressData);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            GroupsCount = lidgrenMsg.ReadInt32();
            if (Groups.Length < GroupsCount)
                Groups = new Group[GroupsCount];

            for (var i = 0; i < GroupsCount; i++)
            {
                if (Groups[i] == null)
                    Groups[i] = new Group();

                Groups[i].Deserialize(lidgrenMsg, dataCompressed);
            }
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
