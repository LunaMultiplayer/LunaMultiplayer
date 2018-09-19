using LunaConfigNode;
using System.Collections.Generic;
using System.Linq;

namespace Server.Structures
{
    public class Vessel
    {
        public Dictionary<string, string> Fields;
        public Dictionary<uint, Part> Parts;
        public Dictionary<string, double> Orbit;
        public Dictionary<string, string> ActionGroups;

        public readonly ConfigNode Discovery;
        public readonly ConfigNode FlightPlan;
        public readonly ConfigNode CtrlState;
        public readonly ConfigNode VesselModules;

        public Vessel(ConfigNode cfgNode)
        {
            Fields = cfgNode.GetAllValues().ToDictionary(k => k.Key, k => k.Value);
            Parts = cfgNode.GetNodes("PART").ToDictionary(n => uint.Parse(n.GetValue("uid")), n => new Part(n));
            Orbit = cfgNode.GetNodes("ORBIT")[0].GetAllValues().ToDictionary(n => n.Key, n=> double.Parse(n.Value));
            ActionGroups = cfgNode.GetNodes("ACTIONGROUPS")[0].GetAllValues().ToDictionary(n => n.Key, n => n.Value);

            Discovery = cfgNode.GetNodes("DISCOVERY")[0];
            FlightPlan = cfgNode.GetNodes("FLIGHTPLAN")[0];
            CtrlState = cfgNode.GetNodes("CTRLSTATE")[0];
            VesselModules = cfgNode.GetNodes("VESSELMODULES")[0];
        }
    }
}
