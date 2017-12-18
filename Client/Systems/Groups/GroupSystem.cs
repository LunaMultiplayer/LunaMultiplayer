using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Data.Groups;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LunaClient.Systems.Groups
{
    public class GroupSystem : MessageSystem<GroupSystem, GroupMessageSender, GroupMessageHandler>
    {
        public ConcurrentDictionary<string, Group> Groups { get; } = new ConcurrentDictionary<string, Group>();

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
                if (!existingVal.Members.Any(m=> m == SettingsSystem.CurrentSettings.PlayerName) && 
                    !existingVal.Invited.Any(m => m == SettingsSystem.CurrentSettings.PlayerName))
                {
                    var expectedGroup = existingVal.Clone();

                    var newInvited = new List<string>(expectedGroup.Invited) {SettingsSystem.CurrentSettings.PlayerName};
                    expectedGroup.Invited = newInvited.ToArray();

                    var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<GroupUpdateMsgData>();
                    msgData.Group = expectedGroup;

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
                //TODO: remove this clone and do as with flags to avoid garbage
                var expectedGroup = existingVal.Clone();

                var newMembers = new List<string>(expectedGroup.Members) { username };
                expectedGroup.Members = newMembers.ToArray();

                var newInvited = new List<string>(expectedGroup.Invited.Except(new []{username}));
                expectedGroup.Invited = newInvited.ToArray();

                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<GroupUpdateMsgData>();
                msgData.Group = expectedGroup;

                MessageSender.SendMessage(msgData);
            }
        }

        public void RemoveMember(string groupName, string username)
        {
            if (Groups.TryGetValue(groupName, out var existingVal)
                && existingVal.Owner == SettingsSystem.CurrentSettings.PlayerName)
            {
                //TODO: remove this clone and do as with flags to avoid garbage
                var expectedGroup = existingVal.Clone();

                var newMembers = new List<string>(expectedGroup.Members.Except(new[] { username })) { username };
                expectedGroup.Members = newMembers.ToArray();

                var newInvited = new List<string>(expectedGroup.Invited.Except(new[] { username }));
                expectedGroup.Invited = newInvited.ToArray();

                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<GroupUpdateMsgData>();
                msgData.Group = expectedGroup;

                MessageSender.SendMessage(msgData);
            }
        }
    }
}
