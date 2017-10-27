using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Groups;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LunaClient.Systems.Groups
{
    public class GroupSystem : MessageSystem<GroupSystem, GroupMessageSender, GroupMessageHandler>
    {
        private static ConcurrentDictionary<string, Group> Groups { get; } = new ConcurrentDictionary<string, Group>();
        private static List<string> PendingInvitations { get; } = new List<string>();

        //when we are syncing, we'll receive a list of group names. 
        //we want to keep retrieving them until we reach a certain total number of groups.
        private static int NumGroups => Groups.Count; 

        protected override void OnDisabled()
        {
            base.OnDisabled();
            Groups.Clear();
        }

        public bool IsCurrentPlayerInGroup(string groupName)
        {
            return Groups[groupName].HasMember(SettingsSystem.CurrentSettings.PlayerName);
        }

        public string[] GetPlayersInGroup(string groupName)
        {
            return Groups[groupName].GetMembers();
        }

        public void RegisterGroup(string groupName, string ownerName)
        {
            Groups[groupName] = new Group(groupName, ownerName);
        }

        public void DeregisterGroup(string groupName)
        {
            Groups[groupName] = null;
        }

        public void Invite(string groupName)
        {
            PendingInvitations.Add(groupName);
        }

        public void AddPlayerToGroup(string groupName, string player)
        {
            if (Groups[groupName] != null)
            {
                Groups[groupName].AddMember(player);
            }
        }
        
        public void KickPlayerFromGroup(string groupName, string player)
        {
            if (Groups[groupName] != null)
            {
                Groups[groupName].RemoveMember(player);
            }
        }

        public bool GroupExists(string groupName)
        {
            return Groups[groupName] != null;
        }
    }
}
