using LunaClient.Base;
using LunaClient.Network;
using LunaCommon.Message.Data;
using System;

namespace LunaClient.Systems.ModApi
{
    public class ModApiSystem : MessageSystem<ModApiSystem, ModApiMessageSender, ModApiMessageHandler>
    {
        #region Base overrides

        public override string SystemName { get; } = nameof(ModApiSystem);

        public override int ExecutionOrder => int.MinValue + 2;

        #endregion

        #region Public methods

        /// <summary>
        /// Sends a mod Message.
        /// </summary>
        /// <param name="modName">Mod Name</param>
        /// <param name="messageData">The message payload</param>
        /// <param name="relay">If set to <c>true</c>, The server will relay the Message to all other authenticated clients</param>
        public void SendModMessage(string modName, byte[] messageData, bool relay)
        {
            SendModMessage(modName, messageData, messageData.Length, relay);
        }

        /// <summary>
        /// Sends a mod Message.
        /// </summary>
        /// <param name="modName">Mod Name</param>
        /// <param name="messageData">The message payload</param>
        /// <param name="numBytes">Number of bytes to take from the array</param>
        /// <param name="relay">If set to <c>true</c>, The server will relay the Message to all other authenticated clients</param>
        public void SendModMessage(string modName, byte[] messageData, int numBytes, bool relay)
        {
            if (modName == null)
                return;
            if (messageData == null)
            {
                LunaLog.LogError($"[LMP]: {modName} attemped to send a null Message");
                return;
            }

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ModMsgData>();

            if (msgData.Data.Length < numBytes)
                msgData.Data = new byte[numBytes];

            Array.Copy(messageData, msgData.Data, numBytes);

            msgData.NumBytes = numBytes;
            msgData.Relay = relay;
            msgData.ModName = modName;

            MessageSender.SendMessage(msgData);
        }

        #endregion
    }
}
