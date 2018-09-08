using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Systems.SettingsSys;
using LunaClient.VesselUtilities;

namespace LunaClient.Systems.SafetyBubble
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
            }
        }
    }
}
