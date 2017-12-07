using System;
using System.Threading;
using System.Threading.Tasks;
using LunaCommon.Enums;
using LunaCommon.Message.Interface;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Plugin;
using Server.Settings;

namespace Server.Server
{
    public class MessageSender
    {
        public static async void StartSendingOutgoingMessages(ClientStructure client)
        {
            while (client.ConnectionStatus == ConnectionStatus.Connected)
            {
                if (client.SendMessageQueue.TryDequeue(out var message) && message != null)
                {
                    SendNetworkMessage(client, message);
                }
                else
                {
                    await Task.Delay(GeneralSettings.SettingsStore.SendReceiveThreadTickMs);
                }
            }
        }

        private static void SendNetworkMessage(ClientStructure client, IServerMessageBase message)
        {
            if (client.ConnectionStatus == ConnectionStatus.Connected)
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