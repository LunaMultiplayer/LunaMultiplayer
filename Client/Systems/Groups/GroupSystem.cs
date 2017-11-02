using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Groups;
using LunaCommon.Message.Data.Groups;
using System.Collections.Concurrent;

namespace LunaClient.Systems.Groups
{
    public class GroupSystem : MessageSystem<GroupSystem, GroupMessageSender, GroupMessageHandler>
    {
        public ConcurrentDictionary<string, Group> Groups { get; } = new ConcurrentDictionary<string, Group>();

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
                    MessageSender.SendMessage(new GroupUpdateMsgData{ Group = expectedGroup });
                }
            }
        }

        public void CreateGroup(string groupName)
        {
            if (!Groups.ContainsKey(groupName))
            {
                MessageSender.SendMessage(new GroupCreateMsgData{ GroupName = SettingsSystem.CurrentSettings.PlayerName });
            }
        }

        public void RemoveGroup(string groupName)
        {
            if (Groups.TryGetValue(groupName, out var existingVal) && existingVal.Owner == SettingsSystem.CurrentSettings.PlayerName)
            {
                MessageSender.SendMessage(new GroupRemoveMsgData{ GroupName = SettingsSystem.CurrentSettings.PlayerName });
            }
        }

        public void AddMember(string groupName, string username)
        {
            if (Groups.TryGetValue(groupName, out var existingVal) 
                && existingVal.Owner == SettingsSystem.CurrentSettings.PlayerName)
            {
                var expectedGroup = existingVal.Clone();
                expectedGroup.Members.Add(username);
                expectedGroup.Invited.Remove(username);
                MessageSender.SendMessage(new GroupUpdateMsgData { Group = expectedGroup });
            }
        }

        public void RemoveMember(string groupName, string username)
        {
            if (Groups.TryGetValue(groupName, out var existingVal)
                && existingVal.Owner == SettingsSystem.CurrentSettings.PlayerName)
            {
                var expectedGroup = existingVal.Clone();
                expectedGroup.Members.Remove(username);
                expectedGroup.Invited.Remove(username);
                MessageSender.SendMessage(new GroupUpdateMsgData { Group = expectedGroup });
            }
        }
    }
}
