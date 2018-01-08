using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Scenario
{
    public abstract class ScenarioBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal ScenarioBaseMsgData() { }
        public override ushort SubType => (ushort)(int)ScenarioMessageType;
        public virtual ScenarioMessageType ScenarioMessageType => throw new NotImplementedException();

        public override string ClassName { get; } = nameof(ScenarioBaseMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            //Nothing to implement here
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            //Nothing to implement here
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return 0;
        }
    }
}
