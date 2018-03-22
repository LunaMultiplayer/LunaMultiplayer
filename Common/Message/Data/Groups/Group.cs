using Lidgren.Network;
using LunaCommon.Message.Base;
using System;
using System.Linq;

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
            if (MemberwiseClone() is Group obj)
            {
                obj.Name = Name.Clone() as string;
                obj.Owner = Owner.Clone() as string;
                obj.Members = Members.Select(m => m.Clone() as string).ToArray();
                obj.Invited = Invited.Select(m => m.Clone() as string).ToArray();

                return obj;
            }

            return null;
        }
        
        public void Serialize(NetOutgoingMessage lidgrenMsg)
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

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            Name = lidgrenMsg.ReadString();
            Owner = lidgrenMsg.ReadString();

            MembersCount = lidgrenMsg.ReadInt32();
            if (Members.Length < MembersCount)
                Members = new string[MembersCount];

            for (var i = 0; i < MembersCount; i++)
                Members[i] = lidgrenMsg.ReadString();

            InvitedCount = lidgrenMsg.ReadInt32();
            if (Invited.Length < InvitedCount)
                Invited = new string[InvitedCount];

            for (var i = 0; i < InvitedCount; i++)
                Invited[i] = lidgrenMsg.ReadString();
        }
        public int GetByteCount()
        {
            return Name.GetByteCount() + Owner.GetByteCount() + 
                sizeof(int) + Members.GetByteCount(MembersCount) +
                sizeof(int) + Invited.GetByteCount(InvitedCount);
        }
    }
}
