using LunaClient.Utilities;
using LunaCommon;

namespace LunaClient.Systems.KerbalSys
{
    public class KerbalStructure
    {
        public string Name => KerbalData.GetValue("name");
        public ConfigNode KerbalData { get; set; }
        public string Hash => Common.CalculateSha256Hash(ConfigNodeSerializer.Serialize(KerbalData));
        public bool Loaded => HighLogic.CurrentGame.CrewRoster.Exists(Name);

        public KerbalStructure(ConfigNode kerbalData)
        {
            KerbalData = kerbalData;
        }
    }
}
