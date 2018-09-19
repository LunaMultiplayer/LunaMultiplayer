using LunaConfigNode;
using System.Collections.Generic;
using System.Linq;

namespace Server.Structures
{


    public class Vessel
    {
        public MixedCollection<string, string> Fields;
        public MixedCollection<uint, Part> Parts;
        public MixedCollection<string, double> Orbit;
        public MixedCollection<string, string> ActionGroups;

        public readonly ConfigNode Discovery;
        public readonly ConfigNode FlightPlan;
        public readonly ConfigNode CtrlState;
        public readonly ConfigNode VesselModules;

        public Vessel(ConfigNode cfgNode)
        {
            Fields = new MixedCollection<string, string>(cfgNode.GetAllValues());
            Parts = new MixedCollection<uint, Part>(cfgNode.GetNodes("PART").Select(n=> new KeyValuePair<uint, Part>(uint.Parse(n.GetValue("uid")), new Part(n))));
            Orbit = new MixedCollection<string, double>(cfgNode.GetNodes("ORBIT").First().GetAllValues().Select(n => new KeyValuePair<string, double>(n.Key, double.TryParse(n.Value, out var obtVal) ? obtVal : 0)));
            ActionGroups = new MixedCollection<string, string>(cfgNode.GetNodes("ACTIONGROUPS").First().GetAllValues());

            Discovery = cfgNode.GetNodes("DISCOVERY").First();
            FlightPlan = cfgNode.GetNodes("FLIGHTPLAN").First();
            CtrlState = cfgNode.GetNodes("CTRLSTATE").First();
            VesselModules = cfgNode.GetNodes("VESSELMODULES").First();
        }
    }
}
