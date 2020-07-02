using System.IO;
using System.Linq;
using LmpClient.Base;
using LmpClient.Localization;
using LmpClient.Systems.SettingsSys;
using LmpClient.Utilities;
using LmpClient.VesselUtilities;

namespace LmpClient.Systems.SafetyBubble
{
    public class SafetyBubbleEvents : SubSystem<SafetyBubbleSystem>
    {
        public void FlightReady()
        {
            //Only show safety bubble text if safety bubble is active and player is spawning a new vessel
            if (VesselCommon.IsSpectating || FlightGlobals.ActiveVessel == null || !FlightGlobals.ActiveVessel.vesselSpawning || SettingsSystem.ServerSettings.SafetyBubbleDistance <= 0) return;

            if (System.IsInSafetyBubble(FlightGlobals.ActiveVessel) && FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH)
            {
                System.DrawSafetyBubble();
            }

            if (FlightGlobals.ActiveVessel.vesselSpawning)
            {
                LunaScreenMsg.PostScreenMessage(LocalizationContainer.ScreenText.SafetyBubble, 10f, ScreenMessageStyle.UPPER_CENTER);
                CoroutineUtil.StartDelayedRoutine(nameof(SafetyBubbleEvents), 
                    ()=> LunaScreenMsg.PostScreenMessage(LocalizationContainer.ScreenText.CheckParts, 15f, ScreenMessageStyle.UPPER_CENTER), 25f);
            }
        }
    }
}
