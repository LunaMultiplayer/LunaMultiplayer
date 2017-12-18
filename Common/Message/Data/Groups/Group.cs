using Lidgren.Network;
using LunaCommon.Message.Base;
using System;

namespace LunaCommon.Message.Data.Groups
{
    [Serializable]
    public class Group
    {
        public string Name;
        public string Owner;

        public int MembersCount;
        public string[] Members = new string[0];

        public int InvitedCount;
        public string[] Invited = new string[0];
        
        public Group Clone()
        {
            return MemberwiseClone() as Group;
        }
        
        public void Serialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            lidgrenMsg.Write(Name);
            lidgrenMsg.Write(Owner);

            lidgrenMsg.Write(MembersCount);
            for (var i = 0; i < MembersCount; i++)
                lidgrenMsg.Write(Members[i]);

            lidgrenMsg.Write(InvitedCount);
            for (var i = 0; i < InvitedCount; i++)
                lidgrenMsg.Write(Invited[i]);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            Name = lidgrenMsg.ReadString();
            Owner = lidgrenMsg.ReadString();

            MembersCount = lidgrenMsg.ReadInt32();
            Members = ArrayPool<string>.Claim(MembersCount);
            for (var i = 0; i < MembersCount; i++)
                Members[i] = lidgrenMsg.ReadString();

            InvitedCount = lidgrenMsg.ReadInt32();
            Invited = ArrayPool<string>.Claim(InvitedCount);
            for (var i = 0; i < InvitedCount; i++)
                Invited[i] = lidgrenMsg.ReadString();
        }

        public void Recycle()
        {
            ArrayPool<string>.Release(ref Members);
            ArrayPool<string>.Release(ref Invited);
        }

        public int GetByteCount(bool dataCompressed)
        {
            return Name.GetByteCount() + Owner.GetByteCount() + 
                sizeof(int) + Members.GetByteCount(MembersCount) +
                sizeof(int) + Invited.GetByteCount(InvitedCount);
        }
    }
}
