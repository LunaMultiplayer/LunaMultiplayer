using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Kerbal
{
    public class KerbalRemoveMsgData : KerbalBaseMsgData
    {
        /// <inheritdoc />
        internal KerbalRemoveMsgData() { }
        public override KerbalMessageType KerbalMessageType => KerbalMessageType.Remove;
        
        public string KerbalName { get; set; }
    }
}