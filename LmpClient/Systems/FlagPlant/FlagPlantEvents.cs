using LmpClient.Base;
using LmpClient.Systems.VesselProtoSys;
using System;

namespace LmpClient.Systems.FlagPlant
{
    public class FlagPlantEvents : SubSystem<FlagPlantSystem>
    {
        public void AfterFlagPlanted(FlagSite data)
        {
            if (data.vessel.id == Guid.Empty)
                data.vessel.id = Guid.NewGuid();

            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(data.vessel, true);
        }
    }
}
