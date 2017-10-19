using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Utilities;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;

namespace LunaClient.Systems.KerbalSys
{
    public class KerbalMessageHandler : SubSystem<KerbalSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as KerbalBaseMsgData;
            if (msgData == null) return;

            switch (msgData.KerbalMessageType)
            {
                case KerbalMessageType.Reply:
                    HandleKerbalReply(msgData as KerbalReplyMsgData);
                    break;
                case KerbalMessageType.Proto:
                    HandleKerbalProto(msgData as KerbalProtoMsgData);
                    break;
                default:
                    LunaLog.LogError("[LMP]: Invalid Kerbal message type");
                    break;
            }
        }

        /// <summary>
        /// Just load the received kerbal into game
        /// </summary>
        /// <param name="messageData"></param>
        private static void HandleKerbalProto(KerbalProtoMsgData messageData)
        {
            var kerbalNode = ConfigNodeSerializer.Deserialize(messageData.KerbalData);
            if (kerbalNode != null)
                System.LoadKerbal(kerbalNode);
            else
                LunaLog.LogError("[LMP]: Failed to load kerbal!");
        }

        /// <summary>
        /// We store all the kerbals in the KerbalProtoQueue dictionary so later once the game starts we load them
        /// </summary>
        /// <param name="messageData"></param>
        private static void HandleKerbalReply(KerbalReplyMsgData messageData)
        {
            foreach (var kerbal in messageData.KerbalsData)
            {
                var kerbalNode = ConfigNodeSerializer.Deserialize(kerbal.Value);
                if (kerbalNode != null)
                    System.KerbalQueue.Enqueue(kerbalNode);
                else
                    LunaLog.LogError("[LMP]: Failed to load kerbal!");
            }

            LunaLog.Log("[LMP]: Kerbals Synced!");
            MainSystem.NetworkState = ClientState.KerbalsSynced;
        }
    }
}
