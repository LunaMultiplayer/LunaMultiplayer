using Lidgren.Network;
using LMP.Server.Client;
using LMP.Server.Context;
using LMP.Server.Log;
using LMP.Server.Message.Reader;
using LMP.Server.Message.Reader.Base;
using LMP.Server.Plugin;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Interface;
using LunaCommon.Time;
using System;
using System.Collections.Generic;

namespace LMP.Server.Server
{
    public class MessageReceiver
    {
        #region Handlers

        private static readonly Dictionary<ClientMessageType, ReaderBase> HandlerDictionary = new Dictionary
            <ClientMessageType, ReaderBase>
        {
            [ClientMessageType.Groups] = new GroupMsgReader(),
            [ClientMessageType.Admin] = new AdminMsgReader(),
            [ClientMessageType.Handshake] = new HandshakeMsgReader(),
            [ClientMessageType.Chat] = new ChatMsgReader(),
            [ClientMessageType.PlayerStatus] = new PlayerStatusMsgReader(),
            [ClientMessageType.PlayerColor] = new PlayerColorMsgReader(),
            [ClientMessageType.Scenario] = new ScenarioDataMsgReader(),
            [ClientMessageType.Kerbal] = new KerbalMsgReader(),
            [ClientMessageType.Settings] = new SettingsMsgReader(),
            [ClientMessageType.Vessel] = new VesselMsgReader(),
            [ClientMessageType.CraftLibrary] = new CraftLibraryMsgReader(),
            [ClientMessageType.Flag] = new FlagSyncMsgReader(),
            [ClientMessageType.Motd] = new MotdMsgReader(),
            [ClientMessageType.Warp] = new WarpControlMsgReader(),
            [ClientMessageType.Lock] = new LockSystemMsgReader(),
            [ClientMessageType.Mod] = new ModDataMsgReader()
        };

        #endregion

        public void ReceiveCallback(ClientStructure client, NetIncomingMessage msg)
        {
            if (client == null || msg.LengthBytes <= 1) return;

            if (client.ConnectionStatus == ConnectionStatus.Connected)
                client.LastReceiveTime = ServerContext.ServerClock.ElapsedMilliseconds;

            var messageBytes = msg.ReadBytes(msg.LengthBytes);

            var message = DeserializeMessage(messageBytes);
            if (message == null) return;

            LmpPluginHandler.FireOnMessageReceived(client, message);
            //A plugin has handled this message and requested suppression of the default behavior
            if (message.Handled) return;

            if (message.VersionMismatch)
            {
                MessageQueuer.SendConnectionEnd(client, $"Version mismatch. Your version does not match the server's version: {LmpVersioning.CurrentVersion}.");
                return;
            }

            //Clients can only send HANDSHAKE until they are Authenticated.
            if (!client.Authenticated && message.MessageType != ClientMessageType.Handshake)
            {
                MessageQueuer.SendConnectionEnd(client, $"You must authenticate before sending a {message.MessageType} message");
                return;
            }

            //Handle the message
            HandlerDictionary[message.MessageType].HandleMessage(client, message.Data);

            //The server does not recycle RECEIVED messages as memory spikes are not so important here
            //Also this would mess with the relay system
            //message.Recycle();
        }

        private static IClientMessageBase DeserializeMessage(byte[] messageBytes)
        {
            try
            {
                return ServerContext.ClientMessageFactory.Deserialize(messageBytes, LunaTime.UtcNow.Ticks) as IClientMessageBase;
            }
            catch (Exception e)
            {
                LunaLog.Error($"Error deserializing message! {e}");
                return null;
            }
        }
    }
}