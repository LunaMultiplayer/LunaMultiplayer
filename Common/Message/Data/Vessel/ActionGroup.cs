using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.Vessel
{
    public class ActionGroup
    {
        public string ActionGroupName;
        public bool State;
        public double Time;

        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(ActionGroupName);
            lidgrenMsg.Write(State);
            lidgrenMsg.Write(Time);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            ActionGroupName = lidgrenMsg.ReadString();
            State = lidgrenMsg.ReadBoolean();
            Time = lidgrenMsg.ReadDouble();
        }

        public int GetByteCount()
        {
            return ActionGroupName.GetByteCount() + sizeof(bool) + sizeof(double);
        }
    }
}
