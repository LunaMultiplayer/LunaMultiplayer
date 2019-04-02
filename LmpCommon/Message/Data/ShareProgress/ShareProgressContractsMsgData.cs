using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Data packet for sending contracts between clients.
    /// </summary>
    public class ShareProgressContractsMsgData : ShareProgressBaseMsgData
    {
        /// <inheritdoc />
        internal ShareProgressContractsMsgData() { }
        public override ShareProgressMessageType ShareProgressMessageType => ShareProgressMessageType.ContractsUpdate;

        public int ContractCount;
        public ContractInfo[] Contracts = new ContractInfo[0];

        public override string ClassName { get; } = nameof(ShareProgressContractsMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(ContractCount);

            for (var i = 0; i < ContractCount; i++)
            {
                Contracts[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            ContractCount = lidgrenMsg.ReadInt32();
            if (Contracts.Length < ContractCount)
                Contracts = new ContractInfo[ContractCount];


            for (var i = 0; i < ContractCount; i++)
            {
                if (Contracts[i] == null)
                    Contracts[i] = new ContractInfo();

                Contracts[i].Deserialize(lidgrenMsg);
            }
        }

        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < ContractCount; i++)
            {
                arraySize += Contracts[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + sizeof(int) + arraySize;
        }
    }
}
