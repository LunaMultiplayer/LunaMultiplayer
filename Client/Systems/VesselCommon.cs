using System;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselChangeSys;
using LunaClient.Systems.VesselDockSys;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselUpdateSys;
using LunaClient.Systems.VesselWarpSys;

namespace LunaClient.Systems
{
    public class VesselCommon
    {
        public static Guid CurrentVesselId => FlightGlobals.ActiveVessel == null ? Guid.Empty : FlightGlobals.ActiveVessel.id;

        public static bool ActiveVesselIsInSafetyBubble()
        {
            return IsInSafetyBubble(FlightGlobals.ActiveVessel.GetWorldPos3D(), FlightGlobals.ActiveVessel.mainBody);
        }

        public static bool IsInSafetyBubble(Vector3d worlPos, CelestialBody body)
        {
            //If not at Kerbin or past ceiling we're definitely clear
            if (body.name != "Kerbin")
                return false;
            var landingPadPosition = body.GetWorldSurfacePosition(-0.0971978130377757, 285.44237039111, 60);
            var runwayPosition = body.GetWorldSurfacePosition(-0.0486001121594686, 285.275552559723, 60);
            var landingPadDistance = Vector3d.Distance(worlPos, landingPadPosition);
            var runwayDistance = Vector3d.Distance(worlPos, runwayPosition);
            return (runwayDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance) ||
                (landingPadDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance);
        }

        public static bool EnableAllSystems
        {
            set
            {
                if (value)
                {
                    VesselLockSystem.Singleton.Enabled = true;
                    VesselUpdateSystem.Singleton.Enabled = true;
                    VesselChangeSystem.Singleton.Enabled = true;
                    VesselProtoSystem.Singleton.Enabled = true;
                    VesselRemoveSystem.Singleton.Enabled = true;
                    VesselDockSystem.Singleton.Enabled = true;
                    VesselWarpSystem.Singleton.Enabled = true;
                }
                else
                {
                    VesselLockSystem.Singleton.Enabled = false;
                    VesselUpdateSystem.Singleton.Enabled = false;
                    VesselChangeSystem.Singleton.Enabled = false;
                    VesselProtoSystem.Singleton.Enabled = false;
                    VesselRemoveSystem.Singleton.Enabled = false;
                    VesselDockSystem.Singleton.Enabled = false;
                    VesselWarpSystem.Singleton.Enabled = false;
                }
            }
        }
    }
}
