using LunaConfigNode;
using LunaConfigNode.CfgNode;
using System.Linq;
using System.Text;

namespace Server.Structures
{
    public class Part
    {
        public MixedCollection<string, string> Fields;
        public MixedCollection<string, ConfigNode> Modules;
        public MixedCollection<string, ConfigNode> Resources;
        public ConfigNode Events;
        public ConfigNode Actions;
        public ConfigNode Effects;
        public ConfigNode Partdata;
        public ConfigNode VesselNaming;

        public Part(ConfigNode cfgNode)
        {
            Fields = new MixedCollection<string, string>(cfgNode.GetAllValues());
            Modules = new MixedCollection<string, ConfigNode>(cfgNode.GetNodes("MODULE").Select(m=> new CfgNodeValue<string, ConfigNode>(m.Value.GetValue("name").Value, m.Value)));
            Resources = new MixedCollection<string, ConfigNode>(cfgNode.GetNodes("RESOURCE").Select(m => new CfgNodeValue<string, ConfigNode>(m.Value.GetValue("name").Value, m.Value)));

            Events = cfgNode.GetNode("EVENTS")?.Value;
            Actions = cfgNode.GetNode("ACTIONS")?.Value;
            Effects = cfgNode.GetNode("EFFECTS")?.Value;
            Partdata = cfgNode.GetNode("PARTDATA")?.Value;
            VesselNaming = cfgNode.GetNode("VESSELNAMING")?.Value;
        }

        public ConfigNode GetSingleModule(string moduleName)
        {
            return Modules.GetSingle(moduleName).Value;
        }
        
        public override string ToString()
        {
            var builder = new StringBuilder();

            CfgNodeWriter.InitializeNode("PART", 1, builder);

            CfgNodeWriter.WriteValues(Fields.GetAll(), 1, builder);

            if (Events!= null) builder.AppendLine(CfgNodeWriter.WriteConfigNode(Events));
            if (Actions != null) builder.AppendLine(CfgNodeWriter.WriteConfigNode(Actions));
            if (Effects != null) builder.AppendLine(CfgNodeWriter.WriteConfigNode(Effects));
            if (Partdata != null) builder.AppendLine(CfgNodeWriter.WriteConfigNode(Partdata));

            foreach (var module in Modules.GetAll())
            {
                builder.AppendLine(CfgNodeWriter.WriteConfigNode(module.Value));
            }

            foreach (var resource in Resources.GetAll())
            {
                builder.AppendLine(CfgNodeWriter.WriteConfigNode(resource.Value));
            }

            if (VesselNaming != null) builder.AppendLine(CfgNodeWriter.WriteConfigNode(VesselNaming));

            CfgNodeWriter.FinishNode(1, builder);

            return builder.ToString();
        }
    }
}
