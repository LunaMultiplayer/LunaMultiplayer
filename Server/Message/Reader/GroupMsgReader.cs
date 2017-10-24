using System;
using LunaCommon.Message.Data.Groups;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Message.Reader.Base;
using LunaServer.System;
using LunaCommon.Message.Server;
using LunaServer.Server;
using System.Collections.Generic;
using LunaCommon.Groups;

namespace LunaServer.Message.Reader
{
    public class GroupMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var data = (GroupBaseMsgData)messageData;
            switch (data.GroupMessageType)
            {
                case GroupMessageType.Invite:
                    {
                        var info = (GroupInviteMsgData)data;
                        if (GroupSystem.IsOwner(info.GroupName, client.PlayerName))
                        {
                            GroupSystem.Invite(info.GroupName, info.AddressedTo);
                            MessageQueuer.SendToClient<GroupSrvMsg>(client, info);
                        }
                    }
                    break;
                case GroupMessageType.Accept:
                    {
                        var info = (GroupAcceptMsgData)data;
                        if (GroupSystem.IsPendingInvitation(info.GroupName, client.PlayerName))
                        {
                            GroupSystem.AddPlayerToGroup(info.GroupName, client.PlayerName);
                            MessageQueuer.SendToClient<GroupSrvMsg>(client, info);
                        }
                    }
                    break;
                case GroupMessageType.Remove:
                    {
                        var info = (GroupRemoveMsgData)data;
                        if (GroupSystem.IsOwner(info.GroupName, client.PlayerName))
                        {
                            GroupSystem.DeregisterGroup(info.GroupName);
                            MessageQueuer.SendToClient<GroupSrvMsg>(client, info);
                        }
                    }
                    break;
                case GroupMessageType.Add:
                    {
                        var info = (GroupAddMsgData)data;
                        if (GroupSystem.NumGroups(client.PlayerName) < 2)
                        {
                            var newMessage = new GroupAddMsgData
                            {
                                GroupName = info.GroupName,
                                Owner = client.PlayerName
                            };
                            GroupSystem.RegisterGroup(info.GroupName, client.PlayerName);
                            MessageQueuer.SendToClient<GroupSrvMsg>(client, newMessage);
                        }
                    }
                    break;
                case GroupMessageType.Kick:
                    {
                        var info = (GroupKickMsgData)data;
                        if (GroupSystem.IsOwner(info.GroupName, client.PlayerName))
                        {
                            GroupSystem.KickPlayerFromGroup(info.GroupName, info.Player);
                            MessageQueuer.SendToClient<GroupSrvMsg>(client, info);
                        }
                    }
                    break;
                case GroupMessageType.ListRequest:
                    {
                        var groupsAndOwners = GroupSystem.GroupsAndOwners();
                        var newMessage = new GroupListResponseMsgData
                        {
                            Groups = groupsAndOwners.Key,
                            Owners = groupsAndOwners.Value
                        };
                        MessageQueuer.SendToClient<GroupSrvMsg>(client, newMessage);
                    }
                    break;
                case GroupMessageType.UpdateRequest:
                    {
                        var info = (GroupUpdateRequestMsgData)data;
                        var g = GroupSystem.GetGroup(info.GroupName);
                        var newMessage = new GroupUpdateResponseMsgData
                        {
                            Owner = g.Owner,
                            Members = g.GetMembers(),
                            Name = g.Name
                        };
                        MessageQueuer.SendToClient<GroupSrvMsg>(client, newMessage);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}