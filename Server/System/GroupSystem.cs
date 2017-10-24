using LunaCommon.Groups;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LunaServer.System
{
    public class GroupSystem
    {
        private static ConcurrentDictionary<string, Group> Groups { get; } = new ConcurrentDictionary<string, Group>();
        private static ConcurrentDictionary<string, List<string>> PendingInvitations { get; } =
            new ConcurrentDictionary<string, List<string>>();

        public static string[] GetPlayersInGroup(string groupName)
        {
            return Groups[groupName].GetMembers();
        }

        public static void RegisterGroup(string groupName, string ownerName)
        {
            Groups[groupName] = new Group(groupName, ownerName);
        }

        public static void DeregisterGroup(string groupName)
        {
            Groups[groupName] = null;
        }

        public static Group GetGroup(string groupName)
        {
            return Groups[groupName];
        }

        public static void Invite(string groupName, string playerName)
        {
            if (PendingInvitations[groupName] == null)
                PendingInvitations[groupName] = new List<string>();
            
            PendingInvitations[groupName].Add(playerName);
        }

        public static void AddPlayerToGroup(string groupName, string player)
        {
            if (Groups[groupName] != null)
            {
                Groups[groupName].AddMember(player);
            }
        }

        public static void KickPlayerFromGroup(string groupName, string player)
        {
            if (Groups[groupName] != null)
            {
                Groups[groupName].RemoveMember(player);
            }
        }

        public static bool GroupExists(string groupName)
        {
            return Groups[groupName] != null;
        }
        
        public static bool IsOwner(string groupName, string playerName)
        {
            return Groups[groupName].Owner == playerName;
        }

        public static bool IsPendingInvitation(string groupName, string playerName)
        {
            return PendingInvitations[groupName] != null && PendingInvitations[groupName].Contains(playerName);
        }

        public static int NumGroups(string playerName)
        {
            return Groups.Values.Sum(g => g.Owner == playerName ? 1 : 0);
        }

        public static KeyValuePair<string[], string[]> GroupsAndOwners()
        {
            var groupNames = Groups.Keys.ToArray();
            var admins = new string[groupNames.Length];

            for (var i = 0; i < groupNames.Length; i++)
            {
                admins[i] = Groups[groupNames[i]].Owner;
            }

            return new KeyValuePair<string[], string[]>(groupNames, admins);
        }
    }
}
