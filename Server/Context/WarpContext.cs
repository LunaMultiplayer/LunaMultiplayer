using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;

namespace Server.Context
{
    public class Subspace
    {
        public int Id { get; set; }
        public double Time { get; set; }
        public string Creator { get; set; }

        public override string ToString()
        {
            return $"{Id}:{Time.ToString(CultureInfo.InvariantCulture)}";
        }

        public Subspace(int id) : this(id, 0, "Server") { }

        public Subspace(int id, double time) : this(id, time, "Server") { }

        public Subspace(int id, double time, string creator)
        {
            Id = id;
            Time = time;
            Creator = creator;
        }
    }

    public class WarpContext
    {
        public static volatile int NextSubspaceId;

        public static readonly ConcurrentDictionary<int, Subspace> Subspaces = new ConcurrentDictionary<int, Subspace>();

        public static Subspace LatestSubspace => Subspaces.OrderByDescending(s => s.Value.Time).Select(s => s.Value).First();
    }
}
