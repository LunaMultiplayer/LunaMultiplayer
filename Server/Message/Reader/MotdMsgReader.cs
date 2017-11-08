using LunaCommon.Message.Data.Motd;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Message.Reader.Base;
using LunaServer.Server;
using LunaServer.Settings;
using System;

namespace LunaServer.Message.Reader
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