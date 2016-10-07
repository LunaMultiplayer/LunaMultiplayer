using System.Collections.Concurrent;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;

namespace LunaClient.Systems.Admin
{
    public class AdminMessageHandler : SubSystem<AdminSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as AdminBaseMsgData;
            if (msgData == null) return;

            switch (msgData.AdminMessageType)
            {
                case AdminMessageType.LIST_REPLY:
                    {
                        var data = (AdminListReplyMsgData)messageData;
                        foreach (var adminName in data.Admins)
                            System.RegisterServerAdmin(adminName);
                        MainSystem.Singleton.NetworkState = ClientState.ADMINS_SYNCED;
                    }
                    break;
                case AdminMessageType.ADD:
                    {
                        var data = (AdminAddMsgData)messageData;
                        System.RegisterServerAdmin(data.PlayerName);
                    }
                    break;
                case AdminMessageType.REMOVE:
                    {
                        var data = (AdminRemoveMsgData)messageData;
                        System.UnregisterServerAdmin(data.PlayerName);
                    }
                    break;
            }
        }
    }
}