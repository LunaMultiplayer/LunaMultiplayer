using LunaCommon.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaServer.System
{
    public class GroupSystem
    {
        private static readonly Dictionary<string, Group> groups = new Dictionary<string, Group>();
        private static readonly Dictionary<string, List<string>> pendingInvitations = new Dictionary<string, List<string>>();

        public static string[] GetPlayersInGroup(string groupName)
        {
            return groups[groupName].GetMembers();
        }

        public static void RegisterGroup(string groupName, string ownerName)
        {
            groups[groupName] = new Group(groupName, ownerName);
        }

        public static void DeregisterGroup(string groupName)
        {
            groups[groupName] = null;
        }

        public static Group GetGroup(string groupName)
        {
            return groups[groupName];
        }

        public static void Invite(string groupName, string playerName)
        {
            if (pendingInvitations[groupName] == null)
            {
                pendingInvitations[groupName] = new List<string>();
            }
            pendingInvitations[groupName].Add(playerName);
        }

        public static void AddPlayerToGroup(string groupName, string player)
        {
            if (groups[groupName] != null)
            {
                groups[groupName].AddMember(player);
            }
        }

        public static void KickPlayerFromGroup(string groupName, string player)
        {
            if (groups[groupName] != null)
            {
                groups[groupName].RemoveMember(player);
            }
        }

        public static bool GroupExists(string groupName)
        {
            return groups[groupName] != null;
        }
        
        public static bool IsOwner(string groupName, string playerName)
        {
            return groups[groupName].Owner == playerName;
        }

        public static bool IsPendingInvitation(string groupName, string playerName)
        {
            return pendingInvitations[groupName] != null && pendingInvitations[groupName].Contains(playerName);
        }

        public static int NumGroups(string playerName)
        {
            int num = 0;
            foreach(Group g in groups.Values)
            {
                num += g.Owner == playerName ? 1 : 0;
            }
            return num;
        }

        public static KeyValuePair<string[], string[]> GroupsAndOwners()
        {
            string[] groupNames = groups.Keys.ToArray();
            string[] admins = new string[groupNames.Length];

            for (int i = 0; i < groupNames.Length; i++)
            {
                admins[i] = groups[groupNames[i]].Owner;
            }

            return new KeyValuePair<string[], string[]>(groupNames, admins);
        }
    }
}
