using System;
using System.Threading;
using LunaCommon.Enums;
using LunaCommon.Message.Interface;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Plugin;
using LunaServer.Settings;

namespace LunaServer.Server
{
    public class MessageSender
    {
        public static void StartSendingOutgoingMessages(ClientStructure client)
        {
            while (client.ConnectionStatus == ConnectionStatus.CONNECTED)
            {
                IServerMessageBase message;
                if (client.SendMessageQueue.TryDequeue(out message) && message != null)
                {
                    SendNetworkMessage(client, message);
                }
                else
                {
                    Thread.Sleep(GeneralSettings.SettingsStore.SendReceiveThreadTickMs);
                }
            }
        }

        private static void SendNetworkMessage(ClientStructure client, IServerMessageBase message)
        {
            if (client.ConnectionStatus == ConnectionStatus.CONNECTED)
            {
                try
                {
                    ServerContext.LidgrenServer.SendMessageToClient(client, message);
                }
                catch (Exception e)
                {
                    ClientException.HandleDisconnectException("Send network message error: ", client, e);
                    return;
                }
            }
            else
            {
                LunaLog.Normal($"Tried to send a message to client {client.PlayerName}, with connection status: {client.ConnectionStatus}");
            }

            LmpPluginHandler.FireOnMessageSent(client, message);
        }
    }
}