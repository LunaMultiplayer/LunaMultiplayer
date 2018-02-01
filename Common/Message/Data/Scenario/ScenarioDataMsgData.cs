using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Scenario
{
    public class ScenarioDataMsgData : ScenarioBaseMsgData
    {
        /// <inheritdoc />
        internal ScenarioDataMsgData() { }
        public override ScenarioMessageType ScenarioMessageType => ScenarioMessageType.Data;

        public int ScenarioCount;
        public ScenarioInfo[] ScenariosData = new ScenarioInfo[0];

        public override string ClassName { get; } = nameof(ScenarioDataMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(ScenarioCount);
            for (var i = 0; i < ScenarioCount; i++)
            {
                ScenariosData[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            ScenarioCount = lidgrenMsg.ReadInt32();
            if (ScenariosData.Length < ScenarioCount)
                ScenariosData = new ScenarioInfo[ScenarioCount];

            for (var i = 0; i < ScenarioCount; i++)
            {
                if (ScenariosData[i] == null)
                    ScenariosData[i] = new ScenarioInfo();

                ScenariosData[i].Deserialize(lidgrenMsg);
            }
        }

        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < ScenarioCount; i++)
            {
                arraySize += ScenariosData[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + sizeof(int) + arraySize;
        }
    }
}
