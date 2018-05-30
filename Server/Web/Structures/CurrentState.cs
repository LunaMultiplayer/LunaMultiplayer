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

        public void Refresh()
        {
            CurrentPlayers.Clear();
            CurrentVessels.Clear();
            CurrentPlayers.AddRange(ServerContext.Clients.Values.Select(v => v.PlayerName));
            CurrentVessels.AddRange(VesselStoreSystem.CurrentVesselsInXmlFormat.Values.Select(v => new VesselInfo(v)));
        }
    }
}
