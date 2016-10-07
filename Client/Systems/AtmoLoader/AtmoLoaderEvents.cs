using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Systems.AtmoLoader
{
    public class AtmoLoaderEvents : SubSystem<AtmoLoaderSystem>
    {
        public void OnVesselUnpack(Vessel vessel)
        {
            if (System.LoadingFlyingVessels.ContainsKey(vessel.id))
            {
                LunaLog.Debug("Atmo load successful: Vessel is off rails");

                var flyingVesselLoad = System.LoadingFlyingVessels[vessel.id];
                flyingVesselLoad.FlyingVessel.Landed = false;
                flyingVesselLoad.FlyingVessel.Splashed = false;
                flyingVesselLoad.FlyingVessel.landedAt = string.Empty;
                flyingVesselLoad.FlyingVessel.situation = Vessel.Situations.FLYING;
                flyingVesselLoad.FlyingVessel.vesselRanges.landed.load = System.LandedLoadDistanceDefault;
                flyingVesselLoad.FlyingVessel.vesselRanges.landed.unload = System.LandedUnloadDistanceDefault;

                //Stop the vessel from exploding while in unpack range.
                //flyingVesselLoad.LastVesselUpdate?.ApplyVesselUpdate();

                System.LoadingFlyingVessels.Remove(vessel.id);
            }
        }

        public void OnVesselPack(Vessel vessel)
        {
            if (vessel.situation == Vessel.Situations.FLYING)
                System.LastPackTime[vessel.id] = Time.realtimeSinceStartup;
        }

        public void OnVesselWillDestroy(Vessel vessel)
        {
            var pilotedByAnotherPlayer = LockSystem.Singleton.LockExists("control-" + vessel.id) &&
                                         !LockSystem.Singleton.LockIsOurs("control-" + vessel.id);
            var updatedByAnotherPlayer = LockSystem.Singleton.LockExists("update-" + vessel.id) &&
                                         !LockSystem.Singleton.LockIsOurs("update-" + vessel.id);

            //Vessel was packed within the last 5 seconds
            if (System.LastPackTime.ContainsKey(vessel.id) &&
                (Time.realtimeSinceStartup - System.LastPackTime[vessel.id] < 5f))
            {
                System.LastPackTime.Remove(vessel.id);
                if ((vessel.situation == Vessel.Situations.FLYING) &&
                    (pilotedByAnotherPlayer || updatedByAnotherPlayer))
                {
                    LunaLog.Debug("AtmoLoad: Saving player vessel getting packed in atmosphere");
                    var pv = vessel.BackupVessel();

                    var savedNode = new ConfigNode();
                    pv.Save(savedNode);

                    VesselProtoSystem.Singleton.VesselLoader.LoadVessel(savedNode, vessel.id);
                }
            }
            if (System.LastPackTime.ContainsKey(vessel.id))
                System.LastPackTime.Remove(vessel.id);
        }

        public void OnGameSceneLoadRequested(GameScenes scene)
        {
            VesselRemoveSystem.Singleton.KillVessels(System.LoadingFlyingVessels.Values.Select(v => v.FlyingVessel).ToArray());
            System.LoadingFlyingVessels.Clear();
        }
    }
}