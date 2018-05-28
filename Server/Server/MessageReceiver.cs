using Lidgren.Network;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Interface;
using LunaCommon.Time;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Message.Reader;
using Server.Message.Reader.Base;
using Server.Plugin;
using System;
using System.Collections.Generic;

namespace Server.Server
{
    public class MessageReceiver
    {
        #region Handlers

        private static readonly Dictionary<ClientMessageType, ReaderBase> HandlerDictionary = new Dictionary
            <ClientMessageType, ReaderBase>
        {
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
            [ClientMessageType.Mod] = new ModDataMsgReader(),
            [ClientMessageType.Groups] = new GroupMsgReader(),
            [ClientMessageType.Facility] = new FacilityMsgReader(),
            [ClientMessageType.Screenshot] = new ScreenshotMsgReader(),
            [ClientMessageType.ShareProgress] = new ShareProgressMsgReader(),
        };

        #endregion

        public void ReceiveCallback(ClientStructure client, NetIncomingMessage msg)
        {
            if (client == null || msg.LengthBytes <= 1) return;

            if (client.ConnectionStatus == ConnectionStatus.Connected)
                client.LastReceiveTime = ServerContext.ServerClock.ElapsedMilliseconds;

            var message = DeserializeMessage(msg);
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
            HandlerDictionary[message.MessageType].HandleMessage(client, message);
        }

        private static IClientMessageBase DeserializeMessage(NetIncomingMessage msg)
        {
            try
            {
                return ServerContext.ClientMessageFactory.Deserialize(msg, LunaNetworkTime.UtcNow.Ticks) as IClientMessageBase;
            }
            catch (Exception e)
            {
                LunaLog.Error($"Error deserializing message! {e}");
                return null;
            }
        }
    }
}
