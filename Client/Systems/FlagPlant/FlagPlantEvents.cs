using LunaClient.Base;
using LunaClient.Systems.VesselProtoSys;
using System;

namespace LunaClient.Systems.FlagPlant
{
    public class FlagPlantEvents : SubSystem<FlagPlantSystem>
    {
        public void AfterFlagPlanted(FlagSite data)
        {
            if(data.vessel.id == Guid.Empty)
                data.vessel.id = Guid.NewGuid();

            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(data.vessel, false);
        }
    }
}
