using Server.Context;
using Server.System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Web.Structures
{
    public class CurrentState
    {
        public List<string> CurrentPlayers { get; } = new List<string>();
        public List<VesselInfo> CurrentVessels { get; } = new List<VesselInfo>();
        public List<Subspace> Subspaces { get; } = new List<Subspace>();

        public void Refresh()
        {
            CurrentPlayers.Clear();
            CurrentVessels.Clear();
            Subspaces.Clear();
            CurrentPlayers.AddRange(ServerContext.Clients.Values.Select(v => v.PlayerName));
            CurrentVessels.AddRange(VesselStoreSystem.CurrentVessels.Values.Select(v => new VesselInfo(v)));
            Subspaces.AddRange(WarpContext.Subspaces.Values);
        }
    }
}
