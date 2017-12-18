using Lidgren.Network;
using LunaCommon.Message.Base;
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

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(ScenarioCount);
            for (var i = 0; i < ScenarioCount; i++)
            {
                ScenariosData[i].Serialize(lidgrenMsg, dataCompressed);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            ScenarioCount = lidgrenMsg.ReadInt32();
            ScenariosData = ArrayPool<ScenarioInfo>.Claim(ScenarioCount);
            for (var i = 0; i < ScenarioCount; i++)
            {
                if (ScenariosData[i] == null)
                    ScenariosData[i] = new ScenarioInfo();

                ScenariosData[i].Deserialize(lidgrenMsg, dataCompressed);
            }
        }

        public override void Recycle()
        {
            base.Recycle();

            for (var i = 0; i < ScenarioCount; i++)
            {
                ScenariosData[i].Recycle();
            }
            ArrayPool<ScenarioInfo>.Release(ref ScenariosData);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            var arraySize = 0;
            for (var i = 0; i < ScenarioCount; i++)
            {
                arraySize += ScenariosData[i].GetByteCount(dataCompressed);
            }

            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + arraySize;
        }
    }
}
