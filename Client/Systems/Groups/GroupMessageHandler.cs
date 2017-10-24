using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Groups;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;

namespace LunaClient.Systems.Groups
{
    public class GroupMessageHandler : SubSystem<GroupSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as GroupBaseMsgData;
            if (msgData == null) return;

            switch (msgData.GroupMessageType)
            {
                case GroupMessageType.Add:
                    {
                        var data = (GroupAddMsgData)messageData;
                        System.RegisterGroup(data.GroupName, data.Owner);
                    }
                    break;
                case GroupMessageType.Remove:
                    {
                        var data = (GroupRemoveMsgData)messageData;
                        System.DeregisterGroup(data.GroupName);
                    }
                    break;
                case GroupMessageType.Invite:
                    {
                        var data = (GroupInviteMsgData)messageData;
                        if (data.AddressedTo == SettingsSystem.CurrentSettings.PlayerName)
                        {
                            System.Invite(data.GroupName);
                        }
                    }
                    break;
                case GroupMessageType.Accept:
                    {
                        var data = (GroupAcceptMsgData)messageData;
                        System.AddPlayerToGroup(data.GroupName, data.AddressedTo);
                    }
                    break;
                case GroupMessageType.Kick:
                    {
                        var data = (GroupKickMsgData)messageData;
                        System.KickPlayerFromGroup(data.GroupName, data.Player);
                    }
                    break;
                case GroupMessageType.ListResponse:
                    {
                        var data = (GroupListResponseMsgData)messageData;

                        if (data.Groups.Length != data.Owners.Length)
                        {
                            LunaLog.LogWarning("Malformed message of type GroupSystem.ListResponse");
                        }
                        else
                        {
                            for(var i = 0; i < data.Groups.Length; i++)
                            {
                                if (!System.GroupExists(data.Groups[i]))
                                {
                                    System.RegisterGroup(data.Groups[i], data.Owners[i]);
                                }
                            }
                        }

                        if (!System.IsSynced)
                        {
                            if (data.Groups.Length == 0) {
                                System.IsSynced = true;
                                MainSystem.NetworkState = ClientState.GroupsSynced;
                            }
                            System.NumGroups = data.Groups.Length;
                            System.NumGroupsSynced = 0;
                            foreach(var groupName in data.Groups)
                            {
                                System.MessageSender.SendMessage(new GroupUpdateRequestMsgData { GroupName = groupName });
                            }
                        }
                    }
                    break;
                case GroupMessageType.UpdateResponse:
                    {
                        var data = (GroupUpdateResponseMsgData)messageData;
                        if (System.NumGroupsSynced < System.NumGroups)
                        {
                            System.NumGroupsSynced += 1;
                        }

                        if (System.NumGroupsSynced == System.NumGroups)
                        {
                            MainSystem.NetworkState = ClientState.GroupsSynced;
                            System.IsSynced = true;
                        }

                        if (!System.GroupExists(data.Name))
                        {
                            System.RegisterGroup(data.Name, data.Owner);
                        }
                        foreach(string member in data.Members)
                        {
                            System.AddPlayerToGroup(data.Name, member);
                        }
                    }
                    break;
            }
        }
    }
}
