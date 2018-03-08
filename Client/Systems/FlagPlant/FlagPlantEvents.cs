using LunaClient.Base;
using LunaClient.Systems.VesselProtoSys;

namespace LunaClient.Systems.FlagPlant
{
    public class FlagPlantEvents : SubSystem<FlagPlantSystem>
    {
        public void AfterFlagPlanted(FlagSite data)
        {
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(data.vessel, true);
        }
    }
}
