using System;
using LunaCommon.Message.Data.Motd;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Message.Reader.Base;
using Server.Server;
using Server.Settings;

namespace Server.Message.Reader
{
    public class MotdMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData message)
        {
            var newMotd = GeneralSettings.SettingsStore.ServerMotd;
                
            if (newMotd.Length > 255)
                newMotd = newMotd.Substring(0, 255); //We don't wanna send a huuuge message

            newMotd = newMotd.Replace("%Name%", client.PlayerName).Replace(@"\n", Environment.NewLine);

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<MotdReplyMsgData>();
            msgData.MessageOfTheDay = newMotd;

            MessageQueuer.SendToClient<MotdSrvMsg>(client, msgData);
        }
    }
}