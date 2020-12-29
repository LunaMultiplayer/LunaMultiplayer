using HarmonyLib;
using KSP.UI.Screens;
using LmpClient.Base;
using LmpClient.Systems.VesselUpdateSys;
using LmpCommon.Locks;
using System.Reflection;
using UnityEngine;

namespace LmpClient.Systems.KscScene
{
    public class KscSceneEvents : SubSystem<KscSceneSystem>
    {
        private static readonly MethodInfo ClearVesselMarkers = typeof(KSCVesselMarkers).GetMethod("ClearVesselMarkers", AccessTools.all);

        public void OnLockAcquire(LockDefinition lockdefinition)
        {
            System.RefreshTrackingStationVessels();
            RefreshMarkers();
        }

        public void OnLockRelease(LockDefinition lockdefinition)
        {
            System.RefreshTrackingStationVessels();
            RefreshMarkers();
        }

        /// <summary>
        /// Sometimes the vessel markers stay there with corrupt values so here we force clearing them
        /// </summary>
        public void OnSceneRequested(GameScenes requestedScene)
        {
            if (requestedScene > GameScenes.SPACECENTER)
            {
                ClearMarkers();
            }
        }

        /// <summary>
        /// Sometimes the vessel markers stay there with corrupt values so here we force clearing them
        /// </summary>
        public void LevelLoaded(GameScenes data)
        {
            if (data == GameScenes.SPACECENTER)
            {
                ClearMarkers();
                RefreshMarkers();
            }
        }

        public void OnVesselCreated(Vessel vessel)
        {
            System.RefreshTrackingStationVessels();
            RefreshMarkers();
        }

        public void OnVesselRename(GameEvents.HostedFromToAction<Vessel, string> pair)
        {
            /**
             * Use this only in GameScenes.TRACKSTATION, because in FLIGHT working VesselUpdateSystem
             */
            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                pair.host.name = pair.to;

                var vesselUpdateMessageSender = new VesselUpdateMessageSender();
                vesselUpdateMessageSender.SendVesselUpdate(pair.host);
            }
        }

        public void VesselInitialized(Vessel vessel, bool fromShipAssembly)
        {
            System.RefreshTrackingStationVessels();
            RefreshMarkers();
        }

        private static void ClearMarkers()
        {
            if (KSCVesselMarkers.fetch)
                ClearVesselMarkers?.Invoke(KSCVesselMarkers.fetch, null);

            foreach (var kscVesselMarker in Object.FindObjectsOfType<KSCVesselMarker>())
            {
                kscVesselMarker.Terminate();
                Object.DestroyImmediate(kscVesselMarker);
            }
        }

        private static void RefreshMarkers()
        {
            if (KSCVesselMarkers.fetch && HighLogic.LoadedScene == GameScenes.SPACECENTER)
                KSCVesselMarkers.fetch.RefreshMarkers();
        }
    }
}
