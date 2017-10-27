using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;

namespace LunaClient.Systems.Admin
{
    public class AdminMessageHandler : SubSystem<AdminSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            if (!(messageData is AdminBaseMsgData msgData)) return;

            switch (msgData.AdminMessageType)
            {
                case AdminMessageType.ListReply:
                    {
                        var data = (AdminListReplyMsgData)messageData;
                        foreach (var adminName in data.Admins)
                            System.RegisterServerAdmin(adminName);
                        MainSystem.NetworkState = ClientState.AdminsSynced;
                    }
                    break;
                case AdminMessageType.Add:
                    {
                        var data = (AdminAddMsgData)messageData;
                        System.RegisterServerAdmin(data.PlayerName);
                    }
                    break;
                case AdminMessageType.Remove:
                    {
                        var data = (AdminRemoveMsgData)messageData;
                        System.UnregisterServerAdmin(data.PlayerName);
                    }
                    break;
            }
        }
    }
}