using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Data packet for sending achievements between clients.
    /// </summary>
    public class ShareProgressAchievementsMsgData : ShareProgressBaseMsgData
    {
        /// <inheritdoc />
        internal ShareProgressAchievementsMsgData() { }
        public override ShareProgressMessageType ShareProgressMessageType => ShareProgressMessageType.AchievementsUpdate;

        public int AchievementsCount;
        public AchievementInfo[] Achievements = new AchievementInfo[0];

        public override string ClassName { get; } = nameof(ShareProgressAchievementsMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(AchievementsCount);

            for (var i = 0; i < AchievementsCount; i++)
            {
                Achievements[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            AchievementsCount = lidgrenMsg.ReadInt32();
            if (Achievements.Length < AchievementsCount)
                Achievements = new AchievementInfo[AchievementsCount];


            for (var i = 0; i < AchievementsCount; i++)
            {
                if (Achievements[i] == null)
                    Achievements[i] = new AchievementInfo();

                Achievements[i].Deserialize(lidgrenMsg);
            }
        }

        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < AchievementsCount; i++)
            {
                arraySize += Achievements[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + sizeof(int) + arraySize;
        }
    }
}
