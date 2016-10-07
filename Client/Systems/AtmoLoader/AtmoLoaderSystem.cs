using System;
using System.Collections.Generic;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselUpdateSys;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Systems.AtmoLoader
{
    public class AtmoLoaderSystem : System<AtmoLoaderSystem>
    {
        #region Private

        private void UpdateVessels()
        {
            foreach (var vesselId in LoadingFlyingVessels.Keys)
            {
                if (!LoadingFlyingVessels.ContainsKey(vesselId)) continue;

                var flyingVesselLoad = LoadingFlyingVessels[vesselId];
                if ((flyingVesselLoad.FlyingVessel == null) ||
                    (flyingVesselLoad.FlyingVessel.state == Vessel.State.DEAD))
                {
                    LunaLog.Debug("AtmoLoad failed: Vessel destroyed");
                    LoadingFlyingVessels.Remove(vesselId);
                    continue;
                }

                if (!FlightGlobals.Vessels.Contains(flyingVesselLoad.FlyingVessel))
                {
                    LunaLog.Debug("AtmoLoad failed: Vessel destroyed");
                    LoadingFlyingVessels.Remove(vesselId);
                    continue;
                }

                if (!LockSystem.Singleton.LockExists("update-" + vesselId) || LockSystem.Singleton.LockIsOurs("update-" + vesselId))
                {
                    LunaLog.Debug("AtmoLoad removed: Vessel stopped being controlled by another player");
                    LoadingFlyingVessels.Remove(vesselId);
                    VesselRemoveSystem.Singleton.KillVessel(flyingVesselLoad.FlyingVessel);
                    continue;
                }

                if (flyingVesselLoad.FlyingVessel.loaded && Time.realtimeSinceStartup - flyingVesselLoad.LastUnpackTime > UnpackInterval)
                {
                    LunaLog.Debug("AtmoLoad attempting to take loaded vessel off rails");
                    flyingVesselLoad.LastUnpackTime = Time.realtimeSinceStartup;
                    try
                    {
                        flyingVesselLoad.FlyingVessel.GoOffRails();
                    }
                    catch (Exception e)
                    {
                        //Just in case, I don't think this can throw but you never really know with KSP.
                        LunaLog.Debug("AtmoLoad failed to take vessel of rails: " + e.Message);
                    }
                    continue;
                }

                if (!flyingVesselLoad.FlyingVessel.packed)
                {
                    LunaLog.Debug("AtmoLoad successful: Vessel is off rails");
                    LoadingFlyingVessels.Remove(vesselId);
                    flyingVesselLoad.FlyingVessel.Landed = false;
                    flyingVesselLoad.FlyingVessel.Splashed = false;
                    flyingVesselLoad.FlyingVessel.landedAt = string.Empty;
                    flyingVesselLoad.FlyingVessel.situation = Vessel.Situations.FLYING;
                    flyingVesselLoad.FlyingVessel.vesselRanges.landed.load = LandedLoadDistanceDefault;
                    flyingVesselLoad.FlyingVessel.vesselRanges.landed.unload = LandedUnloadDistanceDefault;
                    continue;
                }

                var atmoPressure = flyingVesselLoad.FlyingVessel.mainBody.GetPressure(flyingVesselLoad.FlyingVessel.altitude);
                if (atmoPressure < 0.01d)
                {
                    LunaLog.Debug("AtmoLoad successful: Vessel is now safe from atmo");
                    LoadingFlyingVessels.Remove(vesselId);
                    flyingVesselLoad.FlyingVessel.Landed = false;
                    flyingVesselLoad.FlyingVessel.Splashed = false;
                    flyingVesselLoad.FlyingVessel.landedAt = string.Empty;
                    flyingVesselLoad.FlyingVessel.situation = Vessel.Situations.FLYING;
                    flyingVesselLoad.FlyingVessel.vesselRanges.landed.load = LandedLoadDistanceDefault;
                    flyingVesselLoad.FlyingVessel.vesselRanges.landed.unload = LandedUnloadDistanceDefault;
                    continue;
                }

                //flyingVesselLoad.LastVesselUpdate?.ApplyVesselUpdate();
            }
        }

        #endregion

        #region Fields

        public Dictionary<Guid, float> LastPackTime { get; } = new Dictionary<Guid, float>();

        public Dictionary<Guid, FlyingVesselLoad> LoadingFlyingVessels { get; } =
            new Dictionary<Guid, FlyingVesselLoad>();

        public float LandedLoadDistanceDefault { get; } = 2250f;
        public float LandedUnloadDistanceDefault { get; } = 2700f;
        private AtmoLoaderEvents AtmoLoaderEventHandler { get; } = new AtmoLoaderEvents();
        private bool Registered { get; set; }

        private const float UnpackInterval = 3f;

        private bool _enabled;

        public override bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (!_enabled && value)
                {
                    GameEvents.onGameSceneLoadRequested.Add(AtmoLoaderEventHandler.OnGameSceneLoadRequested);
                    GameEvents.onVesselGoOffRails.Add(AtmoLoaderEventHandler.OnVesselUnpack);
                    GameEvents.onVesselGoOnRails.Add(AtmoLoaderEventHandler.OnVesselPack);
                    GameEvents.onVesselWillDestroy.Add(AtmoLoaderEventHandler.OnVesselWillDestroy);
                }
                else if (_enabled && !value)
                {
                    GameEvents.onGameSceneLoadRequested.Remove(AtmoLoaderEventHandler.OnGameSceneLoadRequested);
                    GameEvents.onVesselGoOffRails.Remove(AtmoLoaderEventHandler.OnVesselUnpack);
                    GameEvents.onVesselGoOnRails.Remove(AtmoLoaderEventHandler.OnVesselPack);
                    GameEvents.onVesselWillDestroy.Remove(AtmoLoaderEventHandler.OnVesselWillDestroy);
                }

                _enabled = value;
            }
        }

        #endregion

        #region Base system overrides

        public override void FixedUpdate()
        {
            //base.FixedUpdate();
            //if (Enabled)
            //    UpdateVessels();
        }

        protected override void DoReset()
        {
            Enabled = false;
            if (Registered)
                Registered = false;
        }

        #endregion

        #region Public

        public void SetVesselUpdate(Guid guid, VesselUpdate vesselUpdate)
        {
            if (LoadingFlyingVessels.ContainsKey(guid))
                LoadingFlyingVessels[guid].LastVesselUpdate = vesselUpdate;
        }

        public void AddAtmoLoad(Vessel hackyVessel)
        {
            if (!LoadingFlyingVessels.ContainsKey(hackyVessel.id))
            {
                var hfvl = new FlyingVesselLoad { FlyingVessel = hackyVessel };
                hfvl.FlyingVessel.vesselRanges.landed.load = hfvl.FlyingVessel.vesselRanges.flying.unload - 100f;
                LunaLog.Debug("Default landed unload: " + hfvl.FlyingVessel.vesselRanges.landed.unload);
                hfvl.FlyingVessel.vesselRanges.landed.unload = hfvl.FlyingVessel.vesselRanges.flying.unload;
                hfvl.LastUnpackTime = Time.realtimeSinceStartup;
                LoadingFlyingVessels.Add(hackyVessel.id, hfvl);
            }
        }

        public void ForgetVessel(Vessel hackyVessel)
        {
            if (LoadingFlyingVessels.ContainsKey(hackyVessel.id))
                LoadingFlyingVessels.Remove(hackyVessel.id);
        }

        #endregion
    }
}