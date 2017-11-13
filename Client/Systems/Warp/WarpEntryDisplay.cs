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
        /// <summary>
        /// Get the list of player and subspaces depending on the warp mode
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SubspaceDisplayEntry> GetSubspaceDisplayEntries()
        {
            return SettingsSystem.ServerSettings.WarpMode == WarpMode.Subspace ?
                GetSubspaceDisplayEntriesSubspace() : GetSubspaceDisplayEntriesNoneSubspace();
        }

        #region private methods

        /// <summary>
        /// Retrieve the list of subspaces and players when the warp mode is ADMIN or NONE
        /// </summary>
        private static SubspaceDisplayEntry[] GetSubspaceDisplayEntriesNoneSubspace()
        {
            //TODO: Check if this can be improved as it probably creates a lot of garbage in memory
            var allPlayers = new List<string> { SettingsSystem.CurrentSettings.PlayerName };
            allPlayers.AddRange(System.ClientSubspaceList.Keys);
            allPlayers.Sort(PlayerSorter);

            var sde = new SubspaceDisplayEntry { Players = allPlayers, SubspaceId = 0, SubspaceTime = 0 };
            return new[] { sde };
        }

        /// <summary>
        /// Retrieve the list of subspaces and players when the warp mode is SUBSPACE
        /// </summary>
        private static IEnumerable<SubspaceDisplayEntry> GetSubspaceDisplayEntriesSubspace()
        {
            var groupedPlayers = System.ClientSubspaceList.GroupBy(s => s.Value);
            var subspaceDisplay = new List<SubspaceDisplayEntry>();

            foreach (var subspace in groupedPlayers)
            {
                //TODO: This is really bad in terms of garbage collection.
                //Players is being set as "ToList" wich causes garbase also
                //SubspaceDisplayEntry is created on every ongui.
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
                        subspaceDisplay.Insert(0, newSubspaceDisplay);
                    }
                    else
                    {
                        newSubspaceDisplay.Players.Insert(0, SettingsSystem.CurrentSettings.PlayerName);
                        subspaceDisplay.Insert(0, newSubspaceDisplay);
                    }
                }
                else
                {
                    subspaceDisplay.Add(newSubspaceDisplay);
                }
            }

            return subspaceDisplay;
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
