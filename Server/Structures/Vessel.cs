using LunaConfigNode;
using LunaConfigNode.CfgNode;
using System.Linq;
using System.Text;

namespace Server.Structures
{
    public class Vessel
    {
        public MixedCollection<string, string> Fields;
        public MixedCollection<uint, Part> Parts;
        public MixedCollection<string, string> Orbit;
        public MixedCollection<string, string> ActionGroups;

        public readonly ConfigNode Discovery;
        public readonly ConfigNode FlightPlan;
        public readonly ConfigNode Target;
        public readonly ConfigNode Waypoint;
        public readonly ConfigNode CtrlState;
        public readonly ConfigNode VesselModules;

        public Vessel(ConfigNode cfgNode)
        {
            Fields = new MixedCollection<string, string>(cfgNode.GetAllValues());
            Parts = new MixedCollection<uint, Part>(cfgNode.GetNodes("PART").Select(n => new CfgNodeValue<uint, Part>(uint.Parse(n.Value.GetValues("uid")[0].Value), new Part(n.Value))));
            Orbit = new MixedCollection<string, string>(cfgNode.GetNodes("ORBIT").First().Value.GetAllValues().Select(n => new CfgNodeValue<string, string>(n.Key, n.Value)));
            ActionGroups = new MixedCollection<string, string>(cfgNode.GetNodes("ACTIONGROUPS").First().Value.GetAllValues().Select(n => new CfgNodeValue<string, string>(n.Key, n.Value)));

            Discovery = cfgNode.GetNodes("DISCOVERY").First().Value;
            FlightPlan = cfgNode.GetNodes("FLIGHTPLAN").First().Value;
            Target = cfgNode.GetNodes("TARGET").FirstOrDefault()?.Value;
            Waypoint = cfgNode.GetNodes("WAYPOINT").FirstOrDefault()?.Value;
            CtrlState = cfgNode.GetNodes("CTRLSTATE").First().Value;
            VesselModules = cfgNode.GetNodes("VESSELMODULES").First().Value;
        }

        public Vessel(string vesselText) : this(new ConfigNode(vesselText)) { }

        public Part GetPart(uint partFlightId)
        {
            return Parts.GetSingle(partFlightId).Value;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            CfgNodeWriter.WriteValues(Fields.GetAll(), 0, builder);

            CfgNodeWriter.InitializeNode("ORBIT", 0, builder);
            CfgNodeWriter.WriteValues(Orbit.GetAll(), 1, builder);
            CfgNodeWriter.FinishNode(0, builder);
            builder.AppendLine();

            foreach (var part in Parts.GetAllValues())
            {
                builder.AppendLine(part.ToString());
            }

            CfgNodeWriter.InitializeNode("ACTIONGROUPS", 0, builder);
            CfgNodeWriter.WriteValues(ActionGroups.GetAll(), 1, builder);
            CfgNodeWriter.FinishNode(0, builder);
            builder.AppendLine();

            builder.AppendLine(CfgNodeWriter.WriteConfigNode(Discovery));
            builder.AppendLine(CfgNodeWriter.WriteConfigNode(FlightPlan));
            if (Target != null) builder.AppendLine(CfgNodeWriter.WriteConfigNode(Target));
            if (Waypoint != null) builder.AppendLine(CfgNodeWriter.WriteConfigNode(Waypoint));
            builder.AppendLine(CfgNodeWriter.WriteConfigNode(CtrlState));
            builder.AppendLine(CfgNodeWriter.WriteConfigNode(VesselModules));

            return builder.ToString().TrimEnd();
        }
    }
}
