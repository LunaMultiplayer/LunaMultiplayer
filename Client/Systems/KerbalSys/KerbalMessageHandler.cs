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
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is KerbalBaseMsgData msgData)) return;

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
        private static void HandleKerbalProto(KerbalProtoMsgData messageData)
        {
            ProcessKerbal(messageData.KerbalData);
        }

        /// <summary>
        /// Appends the received kerbal to the dictionary
        /// </summary>
        private static void ProcessKerbal(byte[] kerbalData)
        {
            var kerbalNode = ConfigNodeSerializer.Deserialize(kerbalData);
            if (kerbalNode != null)
            {
                var kerbalStructure = new KerbalStructure(kerbalNode);
                System.Kerbals.AddOrUpdate(kerbalStructure.Name, kerbalStructure, (key, existingVal) => kerbalStructure);
            }
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
                ProcessKerbal(kerbal.Value);
            }

            LunaLog.Log("[LMP]: Kerbals Synced!");
            MainSystem.NetworkState = ClientState.KerbalsSynced;
        }
    }
}
