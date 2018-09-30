using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Scenario
{
    public abstract class ScenarioBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal ScenarioBaseMsgData() { }
        public override bool CompressCondition => false;
        public override ushort SubType => (ushort)(int)ScenarioMessageType;
        public virtual ScenarioMessageType ScenarioMessageType => throw new NotImplementedException();

        public override string ClassName { get; } = nameof(ScenarioBaseMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            //Nothing to implement here
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            //Nothing to implement here
        }

        internal override int InternalGetMessageSize()
        {
            return 0;
        }
    }
}
