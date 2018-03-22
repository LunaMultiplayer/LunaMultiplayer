using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;
using Server.Client;
using Server.Message.Reader.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.System;

namespace Server.Message.Reader
{
    public class ShareProgressMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = (ShareProgressBaseMsgData)message.Data;
            switch (data.ShareProgressMessageType)
            {
                case ShareProgressMessageType.FundsUpdate:
                    ShareProgressSystem.FundsReceived(client, (ShareProgressFundsMsgData)data);
                    break;
                case ShareProgressMessageType.ScienceUpdate:
                    ShareProgressSystem.ScienceReceived(client, (ShareProgressScienceMsgData)data);
                    break;
                case ShareProgressMessageType.ReputationUpdate:
                    ShareProgressSystem.ReputationReceived(client, (ShareProgressReputationMsgData)data);
                    break;
                case ShareProgressMessageType.TechnologyUpdate:
                    ShareProgressSystem.TechnologyReceived(client, (ShareProgressTechnologyMsgData)data);
                    break;
                case ShareProgressMessageType.ContractUpdate:
                    ShareProgressSystem.ContractsReceived(client, (ShareProgressContractMsgData)data);
                    break;
            }
        }
    }
}
