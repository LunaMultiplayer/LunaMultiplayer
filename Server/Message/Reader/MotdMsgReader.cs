using LunaCommon.Message.Data.Motd;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Message.Reader.Base;
using Server.Server;
using Server.Settings;
using System;

namespace Server.Message.Reader
{
    public class MotdMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {                    
            //We don't use this message anymore so we can recycle it
            message.Recycle();

            var newMotd = GeneralSettings.SettingsStore.ServerMotd;
                
            if (newMotd.Length > 255)
                newMotd = newMotd.Substring(0, 255); //We don't wanna send a huuuge message!

            newMotd = newMotd.Replace("%Name%", client.PlayerName).Replace(@"\n", Environment.NewLine);

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<MotdReplyMsgData>();
            msgData.MessageOfTheDay = newMotd;

            MessageQueuer.SendToClient<MotdSrvMsg>(client, msgData);
        }
    }
}