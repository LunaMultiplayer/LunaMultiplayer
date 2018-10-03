using System;
using System.Collections.Concurrent;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Message.Data.Groups;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;

namespace LmpClient.Systems.Groups
{
    public class GroupMessageHandler : SubSystem<GroupSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is GroupBaseMsgData msgData)) return;

            switch (msgData.GroupMessageType)
            {
                case GroupMessageType.ListResponse:
                    {
                        var data = (GroupListResponseMsgData)msgData;
                        for (var i = 0; i < data.GroupsCount; i++)
                        {
                            System.Groups.TryAdd(data.Groups[i].Name, data.Groups[i]);
                        }
                        break;
                    }
                case GroupMessageType.RemoveGroup:
                    {
                        var data = (GroupRemoveMsgData)msgData;
                        System.Groups.TryRemove(data.GroupName, out _);
                        break;
                    }
                case GroupMessageType.GroupUpdate:
                    {
                        var data = (GroupUpdateMsgData)msgData;
                        System.Groups.AddOrUpdate(data.Group.Name, data.Group, (key, existingVal) => data.Group);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
