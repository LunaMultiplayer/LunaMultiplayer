using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LunaServer.Context
{
    public class WarpContext
    {
        public static int NextSubspaceId { get; set; }

        public static ConcurrentDictionary<int, double> Subspaces { get; set; } =
            new ConcurrentDictionary<int, double>();

        public static int LatestSubspace => Subspaces.OrderByDescending(s => s.Value).Select(s => s.Key).First();
    }
}