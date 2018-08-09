using LunaClient.Base;
using LunaClient.VesselUtilities;

namespace LunaClient.Systems.SafetyBubble
{
    public class SafetyBubbleEvents : SubSystem<SafetyBubbleSystem>
    {
        public void LeftSafetyBubble()
        {
            System.DestroySafetyBubble();
        }
        
        public void FlightReady()
        {
            if (VesselCommon.IsSpectating || FlightGlobals.ActiveVessel == null) return;

            if (System.IsInSafetyBubble(FlightGlobals.ActiveVessel) && FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH)
            {
                System.DrawSafetyBubble();
            }
        }
    }
}
