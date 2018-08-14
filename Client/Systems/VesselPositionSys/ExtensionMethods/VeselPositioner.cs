using LunaClient.Extensions;
using LunaClient.Systems.SettingsSys;
using LunaCommon;
using UnityEngine;

namespace LunaClient.Systems.VesselPositionSys.ExtensionMethods
{
    public static class VeselPositioner
    {
        private static Orbit _startOrbit;
        private static Orbit _endOrbit;

        public static void SetVesselPosition(this Vessel vessel, VesselPositionUpdate update, VesselPositionUpdate target, float percentage)
        {
            if (vessel == null || update == null || target == null) return;

            var lerpedBody = percentage < 0.5 ? update.Body : target.Body;

            ApplyOrbitInterpolation(vessel, update, target, lerpedBody, percentage);

            //Do not use CoM. It's not needed and it generate issues when you patch the protovessel with it as it generate weird commnet lines
            //It's important to set the static pressure as otherwise the vessel situation is not updated correctly when
            //Vessel.updateSituation() is called in the Vessel.LateUpdate(). Same applies for landed and splashed
            vessel.staticPressurekPa = FlightGlobals.getStaticPressure(target.LatLonAlt[2], lerpedBody);
            vessel.heightFromTerrain = target.HeightFromTerrain;

            if (!vessel.loaded)
            {
                //DO NOT lerp the latlonalt as otherwise if you are in orbit you will see landed vessels in the map view with weird jittering
                vessel.latitude = target.LatLonAlt[0];
                vessel.longitude = target.LatLonAlt[1];
                vessel.altitude = target.LatLonAlt[2];

                if (vessel.LandedOrSplashed)
                    vessel.SetPosition(lerpedBody.GetWorldSurfacePosition(vessel.latitude, vessel.longitude, vessel.altitude));
            }
            else
            {
                ApplyInterpolationsToLoadedVessel(vessel, update, target, lerpedBody, percentage);
            }
        }

        private static void ApplyOrbitInterpolation(Vessel vessel, VesselPositionUpdate update, VesselPositionUpdate target, CelestialBody lerpedBody, float percentage)
        {
            var startTime = Planetarium.GetUniversalTime();
            var targetTime = Planetarium.GetUniversalTime();

            //Uncomment this if you want to show the other vessels as in their PAST positions
            //This is the old way of how LMP handled the vessels positions when YOUR vessel is in the future
            //Now the vessel positions are advanced and "projected"
            //startTime = update.KspOrbit.epoch;
            //targetTime = target.KspOrbit.epoch;

            //As we are using a position from the PAST, we must compensate the planet rotation in the received LAN parameter
            //rel: https://forum.kerbalspaceprogram.com/index.php?/topic/176149-replaying-orbit-positions-from-the-past/
            var startRotationFixFactor = vessel.situation <= Vessel.Situations.FLYING && update.Body != null && update.Body.referenceBody != null ? 
                update.TimeDifference * 360 / update.Body.SiderealDayLength() : 0;
            var endRotationFixFactor = vessel.situation <= Vessel.Situations.FLYING && update.Body != null && update.Body.referenceBody != null ?
                (target.TimeDifference + update.ExtraInterpolationTime) * 360 / target.Body.SiderealDayLength() : 0;

            _startOrbit = new Orbit(update.Orbit[0], update.Orbit[1], update.Orbit[2], update.Orbit[3] + startRotationFixFactor,
                update.Orbit[4], update.Orbit[5], update.Orbit[6], update.Body);

            _endOrbit = new Orbit(target.Orbit[0], target.Orbit[1], target.Orbit[2], target.Orbit[3] + endRotationFixFactor,
                target.Orbit[4], target.Orbit[5], target.Orbit[6], target.Body);

            var currentPos = _startOrbit.getRelativePositionAtUT(startTime);
            var targetPos = _endOrbit.getRelativePositionAtUT(targetTime);

            var currentVel = _startOrbit.getOrbitalVelocityAtUT(startTime);
            var targetVel = _endOrbit.getOrbitalVelocityAtUT(targetTime);

            var lerpedPos = Vector3d.Lerp(currentPos, targetPos, percentage);
            var lerpedVel = Vector3d.Lerp(currentVel, targetVel, percentage);

            var updateTime = Planetarium.GetUniversalTime();

            //Uncomment this if you want to show the other vessels as in their PAST positions
            //updateTime = LunaMath.Lerp(startTime, targetTime, percentage);

            vessel.orbit.UpdateFromStateVectors(lerpedPos, lerpedVel, lerpedBody, updateTime);
        }

        private static void ApplyInterpolationsToLoadedVessel(Vessel vessel, VesselPositionUpdate update, VesselPositionUpdate target, CelestialBody lerpedBody, float percentage)
        {
            //Do not call vessel.orbitDriver.updateFromParameters()!!
            //It will set the vessel at the CURRENT position and ignore that the orbit is from the PAST!

            var currentSurfaceRelRotation = Quaternion.Slerp(update.SurfaceRelRotation, target.SurfaceRelRotation, percentage);

            //If you don't set srfRelRotation and vessel is packed it won't change it's rotation
            vessel.srfRelRotation = currentSurfaceRelRotation;
            vessel.SetRotation((Quaternion)lerpedBody.rotation * currentSurfaceRelRotation, true);

            vessel.Landed = percentage < 0.5 ? update.Landed : target.Landed;
            vessel.Splashed = percentage < 0.5 ? update.Splashed : target.Splashed;

            vessel.latitude = LunaMath.Lerp(update.LatLonAlt[0], target.LatLonAlt[0], percentage);
            vessel.longitude = LunaMath.Lerp(update.LatLonAlt[1], target.LatLonAlt[1], percentage);
            vessel.altitude = LunaMath.Lerp(update.LatLonAlt[2], target.LatLonAlt[2], percentage);

            //var startLatLonAltPos = update.Body.GetWorldSurfacePosition(update.LatLonAlt[0], update.LatLonAlt[1], update.LatLonAlt[2]);
            //var targetLatLonAltPos = target.Body.GetWorldSurfacePosition(target.LatLonAlt[0], target.LatLonAlt[1], target.LatLonAlt[2]);

            //var startOrbitPos = startOrbit.getPositionAtUT(startOrbit.epoch);
            //var endOrbitPos = endOrbit.getPositionAtUT(endOrbit.epoch);

            if (vessel.situation <= Vessel.Situations.PRELAUNCH || (vessel.situation <= Vessel.Situations.FLYING && SettingsSystem.CurrentSettings.PositionInterpolation))
            {
                vessel.SetPosition(lerpedBody.GetWorldSurfacePosition(vessel.latitude, vessel.longitude, vessel.altitude));
            }
            else
            {
                vessel.SetPosition(vessel.orbit.getPositionAtUT(Planetarium.GetUniversalTime()));
            }

            //Always run this at the end!!
            //Otherwise during docking, the orbital speeds are not displayed correctly and you won't be able to dock
            foreach (var part in vessel.Parts)
                part.ResumeVelocity();
        }
    }
}
