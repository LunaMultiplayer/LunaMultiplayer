using LmpClient.Base;
using LmpClient.Network;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Message.Data.Groups;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LmpClient.Systems.Groups
{
    public class GroupSystem : MessageSystem<GroupSystem, GroupMessageSender, GroupMessageHandler>
    {
        public ConcurrentDictionary<string, Group> Groups { get; } = new ConcurrentDictionary<string, Group>();

        public override string SystemName { get; } = nameof(GroupSystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnDisabled()
        {
            base.OnDisabled();
            Groups.Clear();
        }

        public void JoinGroup(string groupName)
        {
            if (Groups.TryGetValue(groupName, out var existingVal))
            {
                var me = SettingsSystem.CurrentSettings.PlayerName;
                if (existingVal.Members.All(m => m != me) && existingVal.Invited.All(m => m != me))
                {
                    var newInvited = new string[existingVal.Invited.Length + 1];
                    Array.Copy(existingVal.Invited, newInvited, existingVal.Invited.Length);
                    newInvited[existingVal.Invited.Length] = me;

                    var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<GroupUpdateMsgData>();
                    msgData.Group = existingVal.Clone();
                    msgData.Group.Invited = newInvited;

                    MessageSender.SendMessage(msgData);
                }
            }
        }

        public void CreateGroup(string groupName)
        {
            if (!Groups.ContainsKey(groupName))
            {
                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<GroupCreateMsgData>();
                msgData.GroupName = groupName;

                MessageSender.SendMessage(msgData);
            }
        }

        public void RemoveGroup(string groupName)
        {
            if (Groups.TryGetValue(groupName, out var existingVal) && existingVal.Owner == SettingsSystem.CurrentSettings.PlayerName)
            {
                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<GroupRemoveMsgData>();
                msgData.GroupName = groupName;

                MessageSender.SendMessage(msgData);
            }
        }

        public void AddMember(string groupName, string username)
        {
            if (Groups.TryGetValue(groupName, out var existingVal)
                && existingVal.Owner == SettingsSystem.CurrentSettings.PlayerName)
            {
                var newMembers = new string[existingVal.Members.Length + 1];
                Array.Copy(existingVal.Members, newMembers, existingVal.Members.Length);
                newMembers[existingVal.Members.Length] = username;

                var newInvited = existingVal.Invited.Where(m => m != username).ToArray();

                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<GroupUpdateMsgData>();
                msgData.Group = existingVal.Clone();
                msgData.Group.Members = newMembers;
                msgData.Group.Invited = newInvited;

                MessageSender.SendMessage(msgData);
            }
        }

        public void RemoveMember(string groupName, string username)
        {
            if (Groups.TryGetValue(groupName, out var existingVal)
                && existingVal.Owner == SettingsSystem.CurrentSettings.PlayerName)
            {
                var newMembers = existingVal.Members.Where(m => m != username).ToArray();
                var newInvited = existingVal.Invited.Where(m => m != username).ToArray();

                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<GroupUpdateMsgData>();
                msgData.Group = existingVal.Clone();
                msgData.Group.Members = newMembers;
                msgData.Group.Invited = newInvited;

                MessageSender.SendMessage(msgData);
            }
        }
    }
}
