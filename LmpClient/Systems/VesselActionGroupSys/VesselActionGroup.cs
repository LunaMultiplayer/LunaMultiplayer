using LmpClient.Extensions;
using LmpClient.VesselUtilities;
using System;
using System.Globalization;

namespace LmpClient.Systems.VesselActionGroupSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselActionGroup
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;
        public KSPActionGroup ActionGroup;
        public bool Value;

        #endregion
        
        public void ProcessActionGroup()
        {
            var vessel = FlightGlobals.fetch.LmpFindVessel(VesselId);
            if (vessel == null) return;

            if (!VesselCommon.DoVesselChecks(VesselId))
                return;

            //Ignore SAS if we are spectating as it will fight with the FI
            if (ActionGroup == KSPActionGroup.SAS && VesselCommon.IsSpectating && FlightGlobals.ActiveVessel?.id == vessel.id)
                return;

            if (vessel.ActionGroups != null)
            {
                var currentValue = vessel.ActionGroups[ActionGroup];
                if (currentValue != Value)
                {
                    vessel.ActionGroups.ToggleGroup(ActionGroup);
                }
            }

            vessel.protoVessel?.actionGroups.SetValue(ActionGroup.ToString(), $"{Value.ToString(CultureInfo.InvariantCulture)}, 0");
        }
    }
}
