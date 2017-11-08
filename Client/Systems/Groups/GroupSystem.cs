using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Groups;
using LunaCommon.Message.Data.Groups;
using System.Collections.Concurrent;

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
                if (!existingVal.Members.Contains(SettingsSystem.CurrentSettings.PlayerName) && 
                    !existingVal.Invited.Contains(SettingsSystem.CurrentSettings.PlayerName))
                {
                    var expectedGroup = existingVal.Clone();
                    expectedGroup.Invited.Add(SettingsSystem.CurrentSettings.PlayerName);

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
                expectedGroup.Members.Add(username);
                expectedGroup.Invited.Remove(username);

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
                expectedGroup.Members.Remove(username);
                expectedGroup.Invited.Remove(username);

                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<GroupUpdateMsgData>();
                msgData.Group = expectedGroup;

                MessageSender.SendMessage(msgData);
            }
        }
    }
}
