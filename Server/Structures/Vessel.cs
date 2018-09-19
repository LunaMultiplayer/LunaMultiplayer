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
            Orbit = cfgNode.GetNodes("ORBIT").First().GetAllValues().ToDictionary(n => n.Key, n=> double.TryParse(n.Value, out var obtVal) ? obtVal : 0);
            ActionGroups = cfgNode.GetNodes("ACTIONGROUPS").First().GetAllValues().ToDictionary(n => n.Key, n => n.Value);

            Discovery = cfgNode.GetNodes("DISCOVERY").First();
            FlightPlan = cfgNode.GetNodes("FLIGHTPLAN").First();
            CtrlState = cfgNode.GetNodes("CTRLSTATE").First();
            VesselModules = cfgNode.GetNodes("VESSELMODULES").First();
        }
    }
}
