using Server.Context;
using Server.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Web.Structures
{
    public class CurrentState
    {
        public DateTime StartTime { get; set; }
        public List<string> CurrentPlayers { get; } = new List<string>();
        public List<PlayerStatusInfo> PlayerStatuses { get; } = new List<PlayerStatusInfo>();
        public List<VesselInfo> CurrentVessels { get; } = new List<VesselInfo>();
        public List<Subspace> Subspaces { get; } = new List<Subspace>();
        public long BytesUsed { get; set; }

        public void Refresh()
        {
            CurrentPlayers.Clear();
            PlayerStatuses.Clear();
            CurrentVessels.Clear();
            Subspaces.Clear();
            StartTime = TimeContext.StartTime;

            foreach (var client in ServerContext.Clients.Values)
            {
                CurrentPlayers.Add(client.PlayerName);
                PlayerStatuses.Add(new PlayerStatusInfo
                {
                    PlayerName = client.PlayerName,
                    StatusText = client.PlayerStatus.StatusText,
                    VesselText = client.PlayerStatus.VesselText
                });
            }

            CurrentVessels.AddRange(VesselStoreSystem.CurrentVessels.Values.Select(v => new VesselInfo(v)));
            Subspaces.AddRange(WarpContext.Subspaces.Values);
            BytesUsed = Environment.WorkingSet;
        }
    }
}
