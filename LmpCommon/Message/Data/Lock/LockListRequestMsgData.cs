using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Lock
{
    public class LockListRequestMsgData : LockBaseMsgData
    {
        /// <inheritdoc />
        internal LockListRequestMsgData() { }
        public override LockMessageType LockMessageType => LockMessageType.ListRequest;

        public override string ClassName { get; } = nameof(LockListRequestMsgData);
    }
}