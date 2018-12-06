using LmpClient.Systems.TimeSync;
using LmpCommon;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace LmpClient.Systems.VesselPositionSys.ExtensionMethods
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

            ApplyInterpolationsToVessel(vessel, update, target, lerpedBody, percentage);

            vessel.protoVessel.UpdatePositionValues(vessel);
        }

        private static void ApplyOrbitInterpolation(Vessel vessel, VesselPositionUpdate update, VesselPositionUpdate target, CelestialBody lerpedBody, float percentage)
        {
            var currentPos = update.KspOrbit.getRelativePositionAtUT(TimeSyncSystem.UniversalTime);
            var targetPos = target.KspOrbit.getRelativePositionAtUT(TimeSyncSystem.UniversalTime);

            var currentVel = update.KspOrbit.getOrbitalVelocityAtUT(TimeSyncSystem.UniversalTime);
            var targetVel = target.KspOrbit.getOrbitalVelocityAtUT(TimeSyncSystem.UniversalTime);

            var lerpedPos = Vector3d.Lerp(currentPos, targetPos, percentage);
            var lerpedVel = Vector3d.Lerp(currentVel, targetVel, percentage);

            //This call will update the orbit PARAMETERS (ecc, sma, inc, etc) based on the vectors you pass as parameters
            //Bear in mind that this method will NOT reposition the vessel!!
            vessel.orbit.UpdateFromStateVectors(lerpedPos, lerpedVel, lerpedBody, TimeSyncSystem.UniversalTime);
        }

        private static void ApplyInterpolationsToVessel(Vessel vessel, VesselPositionUpdate update, VesselPositionUpdate target, CelestialBody lerpedBody, float percentage)
        {
            var currentSurfaceRelRotation = Quaternion.Slerp(update.SurfaceRelRotation, target.SurfaceRelRotation, percentage);

            //If you don't set srfRelRotation and vessel is packed it won't change it's rotation
            vessel.srfRelRotation = currentSurfaceRelRotation;

            vessel.Landed = percentage < 0.5 ? update.Landed : target.Landed;
            vessel.Splashed = percentage < 0.5 ? update.Splashed : target.Splashed;

            vessel.latitude = LunaMath.Lerp(update.LatLonAlt[0], target.LatLonAlt[0], percentage);
            vessel.longitude = LunaMath.Lerp(update.LatLonAlt[1], target.LatLonAlt[1], percentage);
            vessel.altitude = LunaMath.Lerp(update.LatLonAlt[2], target.LatLonAlt[2], percentage);

            var rotation = (Quaternion)lerpedBody.rotation * currentSurfaceRelRotation;
            var position = vessel.situation <= Vessel.Situations.FLYING ?
                lerpedBody.GetWorldSurfacePosition(vessel.latitude, vessel.longitude, vessel.altitude) :
                vessel.orbit.getPositionAtUT(TimeSyncSystem.UniversalTime);

            SetVesselPositionAndRotation(vessel, position, rotation);
        }

        /// <summary>
        /// Here we set the position and the rotation of every part at once, this is much more optimized than calling SetRotation and SetPosition
        /// </summary>
        [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        private static void SetVesselPositionAndRotation(Vessel vessel, Vector3d position, Quaternion rotation)
        {
            if (!vessel.loaded)
            {
                vessel.vesselTransform.position = position;
                vessel.vesselTransform.rotation = rotation;
            }
            else
            {
                for (var i = 0; i < vessel.parts.Count; i++)
                {
                    vessel.parts[i].partTransform.rotation = rotation * vessel.parts[i].orgRot;
                    if (vessel.packed || vessel.parts[i].physicalSignificance == Part.PhysicalSignificance.FULL)
                    {
                        vessel.parts[i].partTransform.position = position + vessel.vesselTransform.rotation * vessel.parts[i].orgPos;
                    }                        
                    //We always need to set the part velocity (ant it's rigidbody velocity)! Otherwise during dockings it won't be possible to dock
                    vessel.parts[i].ResumeVelocity();
                }
            }
        }
    }
}
