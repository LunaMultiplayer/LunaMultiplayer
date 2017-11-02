using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Groups;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems.Groups
{
    public class GroupMessageHandler : SubSystem<GroupSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            if (!(messageData is GroupBaseMsgData msgData)) return;

            switch (msgData.GroupMessageType)
            {
                case GroupMessageType.ListResponse:
                    {
                        var data = (GroupListResponseMsgData)messageData;
                        foreach (var group in data.Groups)
                        {
                            System.Groups.TryAdd(group.Name, group);
                        }
                        MainSystem.NetworkState = ClientState.GroupsSynced;
                        break;
                    }
                case GroupMessageType.RemoveGroup:
                    {
                        var data = (GroupRemoveMsgData)messageData;
                        System.Groups.TryRemove(data.GroupName, out _);
                        break;
                    }
                case GroupMessageType.GroupUpdate:
                    {
                        var data = (GroupUpdateMsgData)messageData;
                        System.Groups.AddOrUpdate(data.Group.Name, data.Group, (key, existingVal) => data.Group);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
