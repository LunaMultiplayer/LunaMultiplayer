using Contracts;
using Lidgren.Network;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Data packet for sending contracts between clients.
    /// </summary>
    public class ShareProgressContractMsgData : ShareProgressBaseMsgData
    {
        /// <inheritdoc />
        internal ShareProgressContractMsgData() { }
        public override ShareProgressMessageType ShareProgressMessageType => ShareProgressMessageType.ContractUpdate;

        public int ContractCount;
        public ContractInfo[] Contracts = new ContractInfo[0];

        public override string ClassName { get; } = nameof(ShareProgressContractMsgData);

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
            
            
            for (int i = 0; i < ContractCount; i++)
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
