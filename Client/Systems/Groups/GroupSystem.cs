using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Groups;

namespace LunaClient.Systems.Groups
{
    class GroupSystem : MessageSystem<GroupSystem, GroupMessageSender, GroupMessageHandler>
    {
        Dictionary<string, Group> groups { get; set; } = new Dictionary<string, Group>();
        List<string> pendingInvitations { get; } = new List<string>();

        public int NumGroups = 0; // when we are syncing, we'll receive a list of group names. we want to keep retrieving them until we reach a certain total number of groups.
        public int NumGroupsSynced = 0;
        public bool IsSynced = false;

        protected override void OnDisabled()
        {
            base.OnDisabled();
            groups.Clear();
        }

        public bool IsCurrentPlayerInGroup(string groupName)
        {
            return groups[groupName].HasMember(SettingsSystem.CurrentSettings.PlayerName);
        }

        public string[] GetPlayersInGroup(string groupName)
        {
            return groups[groupName].GetMembers();
        }

        public void RegisterGroup(string groupName, string ownerName)
        {
            groups[groupName] = new Group(groupName, ownerName);
        }

        public void DeregisterGroup(string groupName)
        {
            groups[groupName] = null;
        }

        public void Invite(string groupName)
        {
            pendingInvitations.Add(groupName);
        }

        public void AddPlayerToGroup(string groupName, string player)
        {
            if (groups[groupName] != null)
            {
                groups[groupName].AddMember(player);
            }
        }
        
        public void KickPlayerFromGroup(string groupName, string player)
        {
            if (groups[groupName] != null)
            {
                groups[groupName].RemoveMember(player);
            }
        }

        public bool GroupExists(string groupName)
        {
            return groups[groupName] != null;
        }
    }
}
