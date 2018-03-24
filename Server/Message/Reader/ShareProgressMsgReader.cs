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
                    ShareFundsSystem.FundsReceived(client, (ShareProgressFundsMsgData)data);
                    break;
                case ShareProgressMessageType.ScienceUpdate:
                    ShareScienceSystem.ScienceReceived(client, (ShareProgressScienceMsgData)data);
                    break;
                case ShareProgressMessageType.ReputationUpdate:
                    ShareReputationSystem.ReputationReceived(client, (ShareProgressReputationMsgData)data);
                    break;
                case ShareProgressMessageType.TechnologyUpdate:
                    ShareTechnologySystem.TechnologyReceived(client, (ShareProgressTechnologyMsgData)data);
                    break;
                case ShareProgressMessageType.ContractsUpdate:
                    ShareContractsSystem.ContractsReceived(client, (ShareProgressContractsMsgData)data);
                    break;
                case ShareProgressMessageType.AchievementsUpdate:
                    ShareAchievementsSystem.AchievementsReceived(client, (ShareProgressAchievementsMsgData)data);
                    break;
            }
        }
    }
}
