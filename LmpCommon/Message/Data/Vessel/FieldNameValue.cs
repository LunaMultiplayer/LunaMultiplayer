using Lidgren.Network;
using LmpCommon.Message.Base;

namespace LmpCommon.Message.Data.Vessel
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
