using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.Vessel
{
    public class FieldNameValue
    {
        public string FieldName;
        public string Value;
        
        public void Serialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(FieldName);
            lidgrenMsg.Write(Value);
        }

        public void Deserialize(NetIncomingMessage lidgrenMsg)
        {
            FieldName = lidgrenMsg.ReadString();
            Value = lidgrenMsg.ReadString();
        }

        public int GetByteCount()
        {
            return FieldName.GetByteCount() + Value.GetByteCount();
        }
    }
}
