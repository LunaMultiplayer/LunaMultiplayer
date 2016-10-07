using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Message.Reader;
using LunaServer.Message.Reader.Base;
using LunaServer.Plugin;
using LunaServer.System;
using Lidgren.Network;

namespace LunaServer.Server
{
    public class MessageReceiver
    {
        #region Handlers

        private static readonly Dictionary<ClientMessageType, ReaderBase> HandlerDictionary = new Dictionary
            <ClientMessageType, ReaderBase>
        {
            [ClientMessageType.ADMIN] = new AdminMsgReader(),
            [ClientMessageType.HANDSHAKE] = new HandshakeMsgReader(),
            [ClientMessageType.CHAT] = new ChatMsgReader(),
            [ClientMessageType.PLAYER_STATUS] = new PlayerStatusMsgReader(),
            [ClientMessageType.PLAYER_COLOR] = new PlayerColorMsgReader(),
            [ClientMessageType.SCENARIO] = new ScenarioDataMsgReader(),
            [ClientMessageType.SYNC_TIME] = new SyncTimeRequestMsgReader(),
            [ClientMessageType.KERBAL] = new KerbalMsgReader(),
            [ClientMessageType.SETTINGS] = new SettingsMsgReader(),
            [ClientMessageType.VESSEL] = new VesselMsgReader(),
            [ClientMessageType.CRAFT_LIBRARY] = new CraftLibraryMsgReader(),
            [ClientMessageType.FLAG] = new FlagSyncMsgReader(),
            [ClientMessageType.MOTD] = new MotdMsgReader(),
            [ClientMessageType.WARP] = new WarpControlMsgReader(),
            [ClientMessageType.LOCK] = new LockSystemMsgReader(),
            [ClientMessageType.MOD] = new ModDataMsgReader()
        };

        #endregion

        public void ReceiveCallback(ClientStructure client, NetIncomingMessage msg)
        {
            if (client == null || msg.LengthBytes <= 1) return;

            if (client.ConnectionStatus == ConnectionStatus.CONNECTED)
                client.LastReceiveTime = ServerContext.ServerClock.ElapsedMilliseconds;

            var messageBytes = msg.ReadBytes(msg.LengthBytes);
            var message = ServerContext.ClientMessageFactory.Deserialize(messageBytes, DateTime.UtcNow.Ticks) as IClientMessageBase;

            if (message == null)
            {
                LunaLog.Error("Error deserializing message!");
                return;
            }

            LmpPluginHandler.FireOnMessageReceived(client, message);
            //A plugin has handled this message and requested suppression of the default behavior
            if (message.Handled) return;

            if (message.VersionMismatch)
            {
                MessageQueuer.SendConnectionEnd(client, $"Version mismatch. Yours: {message.Data.Version} Server: {VersionInfo.VersionNumber}");
                return;
            }

            //Clients can only send HANDSHAKE until they are Authenticated.
            if (!client.Authenticated && message.MessageType != ClientMessageType.HANDSHAKE)
            {
                MessageQueuer.SendConnectionEnd(client, $"You must authenticate before sending a {message.MessageType} message");
                return;
            }

            //Handle the message
            HandlerDictionary[message.MessageType].HandleMessage(client, message.Data);
        }
    }
}