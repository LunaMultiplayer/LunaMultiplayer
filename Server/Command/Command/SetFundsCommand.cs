using LmpCommon.Message.Data.Chat;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Server;
using Server.Command.Command.Base;
using Server.Command.Common;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.Settings.Structures;
using Server.System.Scenario;

namespace Server.Command.Command
{
    public class SetFundsCommand : SimpleCommand
    {
        //Executes the SetFundsCommand
        public override bool Execute(string commandArgs)
        {
            //Check parameter
            CommandSystemHelperMethods.SplitCommandParamArray(commandArgs, out var parameters);
            if (!CheckParameter(parameters)) return false;
            var funds = parameters[0];
            var isDouble = double.TryParse(funds, out var dFunds);
            if (isDouble)
            {
                //Set funds
                SetFunds(dFunds);
                return true;
            }
            else
            {
                LunaLog.Error($"Syntax error. Use valid number as parameter!");
                return false;
            }
        }

        //Sets given funds value
        private static void SetFunds(double funds)
        {
            //Fund update to server
            ScenarioDataUpdater.WriteFundsDataToFile(funds);
            //Fund update to all other clients
            var data = ServerContext.ServerMessageFactory.CreateNewMessageData<ShareProgressFundsMsgData>();
            data.Funds = funds;
            data.Reason = "Server Command";
            MessageQueuer.SendToAllClients<ShareProgressSrvMsg>(data);
            LunaLog.Debug($"Funds received: {data.Funds} Reason: {data.Reason}");
            ScenarioDataUpdater.WriteFundsDataToFile(data.Funds);
            //var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ChatMsgData>();
            //msgData.From = GeneralSettings.SettingsStore.ConsoleIdentifier;
            //msgData.Text = "Funds were changed to " + funds.ToString();
            //MessageQueuer.SendToAllClients<ChatSrvMsg>(msgData);
        }

        //Checks the given parameter
        private static bool CheckParameter(string[] parameters)
        {
            if (parameters == null || parameters.Length != 1)
            {
                LunaLog.Error($"Syntax error. Use valid number as parameter!");
                return false;
            }
            return true;
        }
    }
}
