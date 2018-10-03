using System.Collections.Concurrent;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Utilities;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Kerbal;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;

namespace LmpClient.Systems.KerbalSys
{
    public class KerbalMessageHandler : SubSystem<KerbalSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is KerbalBaseMsgData msgData)) return;

            switch (msgData.KerbalMessageType)
            {
                case KerbalMessageType.Remove:
                    System.KerbalsToRemove.Enqueue(((KerbalRemoveMsgData)msgData).KerbalName);
                    break;
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
        /// Just load the received kerbal into game and refresh dialogs
        /// </summary>
        private static void HandleKerbalProto(KerbalProtoMsgData messageData)
        {
            ProcessKerbal(messageData.Kerbal.KerbalData, messageData.Kerbal.NumBytes);
        }

        /// <summary>
        /// Appends the received kerbal to the dictionary
        /// </summary>
        private static void ProcessKerbal(byte[] kerbalData, int numBytes)
        {
            var kerbalNode = ConfigNodeSerializer.Deserialize(kerbalData, numBytes);
            if (kerbalNode != null)
            {
                System.KerbalsToProcess.Enqueue(kerbalNode);
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
            for (var i = 0; i < messageData.KerbalsCount; i++)
            {
                ProcessKerbal(messageData.Kerbals[i].KerbalData, messageData.Kerbals[i].NumBytes);
            }

            LunaLog.Log("[LMP]: Kerbals Synced!");
            MainSystem.NetworkState = ClientState.KerbalsSynced;
        }
    }
}
