using Lidgren.Network;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Data packet for sending milestones between clients.
    /// </summary>
    public class ShareProgressMilestoneMsgData : ShareProgressBaseMsgData
    {
        /// <inheritdoc />
        internal ShareProgressMilestoneMsgData() { }
        public override ShareProgressMessageType ShareProgressMessageType => ShareProgressMessageType.MilestoneUpdate;

        public int MilestoneCount;
        public MilestoneInfo[] Milestones = new MilestoneInfo[0];

        public override string ClassName { get; } = nameof(ShareProgressContractMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(MilestoneCount);

            for (var i = 0; i < MilestoneCount; i++)
            {
                Milestones[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            MilestoneCount = lidgrenMsg.ReadInt32();
            if (Milestones.Length < MilestoneCount)
                Milestones = new MilestoneInfo[MilestoneCount];


            for (var i = 0; i < MilestoneCount; i++)
            {
                if (Milestones[i] == null)
                    Milestones[i] = new MilestoneInfo();

                Milestones[i].Deserialize(lidgrenMsg);
            }
        }

        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < MilestoneCount; i++)
            {
                arraySize += Milestones[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + sizeof(int) + arraySize;
        }
    }
}
