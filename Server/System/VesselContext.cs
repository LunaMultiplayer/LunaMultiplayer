using System;
using System.Collections.Generic;

namespace LunaServer.System
{
    public static class VesselContext
    {
        public static List<Guid> RemovedVessels { get; } = new List<Guid>();
    }
}
