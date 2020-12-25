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
        public List<VesselInfo> CurrentVessels { get; } = new List<VesselInfo>();
        public List<Subspace> Subspaces { get; } = new List<Subspace>();
        public int MemBytesUsed { get; }

        public void Refresh()
        {
            CurrentPlayers.Clear();
            CurrentVessels.Clear();
            Subspaces.Clear();
            StartTime = TimeContext.StartTime;
            CurrentPlayers.AddRange(ServerContext.Clients.Values.Select(v => v.PlayerName));
            CurrentVessels.AddRange(VesselStoreSystem.CurrentVessels.Values.Select(v => new VesselInfo(v)));
            Subspaces.AddRange(WarpContext.Subspaces.Values);
            BytesUsed = Environment.WorkingSet;
        }
        
        
    }
}
