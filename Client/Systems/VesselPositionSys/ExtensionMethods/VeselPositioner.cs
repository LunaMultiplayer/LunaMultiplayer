using LunaCommon;
using UnityEngine;

namespace LunaClient.Systems.VesselPositionSys.ExtensionMethods
{
    public static class VeselPositioner
    {
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

            var currentPos = update.KspOrbit.getRelativePositionAtUT(startTime);
            var targetPos = target.KspOrbit.getRelativePositionAtUT(targetTime);

            var currentVel = update.KspOrbit.getOrbitalVelocityAtUT(startTime);
            var targetVel = target.KspOrbit.getOrbitalVelocityAtUT(targetTime);

            var lerpedPos = Vector3d.Lerp(currentPos, targetPos, percentage);
            var lerpedVel = Vector3d.Lerp(currentVel, targetVel, percentage);

            var updateTime = Planetarium.GetUniversalTime();

            //Uncomment this if you want to show the other vessels as in their PAST positions
            //updateTime = LunaMath.Lerp(startTime, targetTime, percentage);

            vessel.orbit.UpdateFromStateVectors(lerpedPos, lerpedVel, lerpedBody, updateTime);
        }

        private static void ApplyInterpolationsToLoadedVessel(Vessel vessel, VesselPositionUpdate update, VesselPositionUpdate target, CelestialBody lerpedBody, float percentage)
        {
            var currentSurfaceRelRotation = Quaternion.Slerp(update.SurfaceRelRotation, target.SurfaceRelRotation, percentage);

            //If you don't set srfRelRotation and vessel is packed it won't change it's rotation
            vessel.srfRelRotation = currentSurfaceRelRotation;
            vessel.SetRotation((Quaternion)lerpedBody.rotation * currentSurfaceRelRotation, true);

            vessel.Landed = percentage < 0.5 ? update.Landed : target.Landed;
            vessel.Splashed = percentage < 0.5 ? update.Splashed : target.Splashed;

            vessel.latitude = LunaMath.Lerp(update.LatLonAlt[0], target.LatLonAlt[0], percentage);
            vessel.longitude = LunaMath.Lerp(update.LatLonAlt[1], target.LatLonAlt[1], percentage);
            vessel.altitude = LunaMath.Lerp(update.LatLonAlt[2], target.LatLonAlt[2], percentage);

            if (vessel.situation <= Vessel.Situations.FLYING)
            {
                //If spectating, directly get the vector from the lerped lat,lon,alt. This method is more reliable but is not as fluid as using a lerped vector
                var position = FlightGlobals.ActiveVessel?.id == vessel.id ? lerpedBody.GetWorldSurfacePosition(vessel.latitude, vessel.longitude, vessel.altitude) :
                    Vector3d.Lerp(update.LatLonAltPos, target.LatLonAltPos, percentage);

                if (vessel.situation <= Vessel.Situations.PRELAUNCH)
                {
                    var currentVelocity = Vector3d.Lerp(update.Velocity, target.Velocity, percentage);

                    vessel.SetWorldVelocity(currentVelocity);
                    vessel.velocityD = currentVelocity;
                }

                vessel.SetPosition(position);
            }
            else
            {
                vessel.orbitDriver.updateFromParameters();
            }
        }
    }
}
