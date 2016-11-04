using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaServer.System
{
    public static class VesselContext
    {
        public static List<Guid> RemovedVessels { get; } = new List<Guid>();
    }
}
