using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LunaServer.Context
{
    public class WarpContext
    {
        public static int NextSubspaceId { get; set; }

        public static ConcurrentDictionary<int, double> Subspaces { get; set; } =
            new ConcurrentDictionary<int, double>();

        public static ConcurrentDictionary<string, int> OfflinePlayerSubspaces { get; set; } =
            new ConcurrentDictionary<string, int>();
        
        public static List<string> IgnoreList { get; set; }
        public static object ListLock { get; } = new object();
    }
}