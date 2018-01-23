using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
using System.Collections.Generic;
using System.Linq;

namespace LunaClient.Systems.Warp
{
    /// <summary>
    /// This subsystem is in charge of sending to the status window the users and it's subspaces
    /// </summary>
    public class WarpEntryDisplay : SubSystem<WarpSystem>
    {
        private static readonly List<SubspaceDisplayEntry> SubspaceEntries = new List<SubspaceDisplayEntry>();

        /// <summary>
        /// Get the list of player and subspaces depending on the warp mode
        /// </summary>
        /// <returns></returns>
        public List<SubspaceDisplayEntry> GetSubspaceDisplayEntries()
        {
            if (SettingsSystem.ServerSettings.WarpMode == WarpMode.Subspace)
                FillSubspaceDisplayEntriesSubspace();
            else
                FillSubspaceDisplayEntriesNoneSubspace();

            return SubspaceEntries;
        }

        #region private methods

        /// <summary>
        /// Retrieve the list of subspaces and players when the warp mode is ADMIN or NONE
        /// </summary>
        private static void FillSubspaceDisplayEntriesNoneSubspace()
        {
            if (SubspaceEntries.Count != 1 || System.ClientSubspaceList.Keys.Count + 1 != SubspaceEntries[0].Players.Count)
            {
                SubspaceEntries.Clear();

                var allPlayers = new List<string> { SettingsSystem.CurrentSettings.PlayerName };
                allPlayers.AddRange(System.ClientSubspaceList.Keys);
                allPlayers.Sort(PlayerSorter);

                SubspaceEntries.Add(new SubspaceDisplayEntry
                {
                    Players = allPlayers,
                    SubspaceId = 0,
                    SubspaceTime = 0
                });
            }
        }

        /// <summary>
        /// Retrieve the list of subspaces and players when the warp mode is SUBSPACE
        /// </summary>
        private static void FillSubspaceDisplayEntriesSubspace()
        {
            //Redo the list only if the subspaces have changed.
            if (PlayersInSubspacesHaveChanged())
            {
                SubspaceEntries.Clear();
                var groupedPlayers = System.ClientSubspaceList.GroupBy(s => s.Value);
                foreach (var subspace in groupedPlayers)
                {
                    var newSubspaceDisplay = new SubspaceDisplayEntry
                    {
                        SubspaceTime = System.GetSubspaceTime(subspace.Key),
                        SubspaceId = subspace.Key,
                        Players = subspace.Select(u => u.Key).ToList()
                    };

                    if (subspace.Key == System.CurrentSubspace)
                    {
                        if (subspace.Select(v => v.Key).Contains(SettingsSystem.CurrentSettings.PlayerName))
                        {
                            SubspaceEntries.Insert(0, newSubspaceDisplay);
                        }
                        else
                        {
                            newSubspaceDisplay.Players.Insert(0, SettingsSystem.CurrentSettings.PlayerName);
                            SubspaceEntries.Insert(0, newSubspaceDisplay);
                        }
                    }
                    else
                    {
                        SubspaceEntries.Add(newSubspaceDisplay);
                    }
                }
            }
        }

        private static bool PlayersInSubspacesHaveChanged()
        {
            //We add 1 as subspace always contain the -1 subspace
            if (SubspaceEntries.Count + 1 != System.Subspaces.Count)
                return true;

            for (var i = 0; i < SubspaceEntries.Count; i++)
            {
                for (var j = 0; j < SubspaceEntries[i].Players.Count; j++)
                {
                    var player = SubspaceEntries[i].Players[j];
                    var expectedSubspace = SubspaceEntries[i].SubspaceId;
                    if (!System.ClientSubspaceList.TryGetValue(player, out var realSubspace) || realSubspace != expectedSubspace)
                        return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Sorts the players
        /// </summary>
        private static int PlayerSorter(string lhs, string rhs)
        {
            var ourName = SettingsSystem.CurrentSettings.PlayerName;
            if (lhs == ourName)
                return -1;

            return rhs == ourName ? 1 : string.CompareOrdinal(lhs, rhs);
        }

        #endregion
    }
}
