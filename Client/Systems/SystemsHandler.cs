using LunaClient.Base.Interface;
using LunaClient.Systems.Admin;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Chat;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.Facility;
using LunaClient.Systems.Flag;
using LunaClient.Systems.GameScene;
using LunaClient.Systems.Groups;
using LunaClient.Systems.Handshake;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Lock;
using LunaClient.Systems.Mod;
using LunaClient.Systems.ModApi;
using LunaClient.Systems.Motd;
using LunaClient.Systems.Network;
using LunaClient.Systems.Ping;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.PlayerConnection;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Toolbar;
using LunaClient.Systems.VesselDockSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselImmortalSys;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselPartModuleSyncSys;
using LunaClient.Systems.VesselPositionSys;
using LunaClient.Systems.VesselPrecalcSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselResourceSys;
using LunaClient.Systems.VesselStateSys;
using LunaClient.Systems.VesselSwitcherSys;
using LunaClient.Systems.VesselSyncSys;
using LunaClient.Systems.VesselUpdateSys;
using LunaClient.Systems.Warp;
using System;
using UnityEngine;

// ReSharper disable ForCanBeConvertedToForeach

namespace LunaClient.Systems
{
    public static class SystemsHandler
    {
        private static readonly ISystem[] Systems =
        {
            SystemsContainer.Get<NetworkSystem>(),
            SystemsContainer.Get<ModSystem>(),
            SystemsContainer.Get<ModApiSystem>(),
            SystemsContainer.Get<HandshakeSystem>(),
            SystemsContainer.Get<TimeSyncerSystem>(),
            SystemsContainer.Get<PingSystem>(),
            SystemsContainer.Get<KerbalSystem>(),
            SystemsContainer.Get<VesselLockSystem>(),
            SystemsContainer.Get<GameSceneSystem>(),
            SystemsContainer.Get<VesselSyncSystem>(),
            SystemsContainer.Get<VesselPositionSystem>(),
            SystemsContainer.Get<VesselFlightStateSystem>(),
            SystemsContainer.Get<VesselPartModuleSyncSystem>(),
            SystemsContainer.Get<VesselUpdateSystem>(),
            SystemsContainer.Get<VesselProtoSystem>(),
            SystemsContainer.Get<VesselRemoveSystem>(),
            SystemsContainer.Get<VesselImmortalSystem>(),
            SystemsContainer.Get<VesselDockSystem>(),
            SystemsContainer.Get<VesselSwitcherSystem>(),
            SystemsContainer.Get<VesselPrecalcSystem>(),
            SystemsContainer.Get<VesselStateSystem>(),
            SystemsContainer.Get<VesselResourceSystem>(),
            SystemsContainer.Get<WarpSystem>(),
            SystemsContainer.Get<LockSystem>(),
            SystemsContainer.Get<SettingsSystem>(),
            SystemsContainer.Get<AsteroidSystem>(),
            SystemsContainer.Get<StatusSystem>(),
            SystemsContainer.Get<ChatSystem>(),
            SystemsContainer.Get<AdminSystem>(),
            SystemsContainer.Get<PlayerColorSystem>(),
            SystemsContainer.Get<PlayerConnectionSystem>(),
            SystemsContainer.Get<MotdSystem>(),
            SystemsContainer.Get<CraftLibrarySystem>(),
            SystemsContainer.Get<FlagSystem>(),
            SystemsContainer.Get<ScenarioSystem>(),
            SystemsContainer.Get<ToolbarSystem>(),
            SystemsContainer.Get<GroupSystem>(),
            SystemsContainer.Get<FacilitySystem>()
        };

        /// <summary>
        /// Call all the fixed updates of the systems
        /// </summary>
        public static void FixedUpdate()
        {
            for (var i = 0; i < Systems.Length; i++)
            {
                try
                {
                    Profiler.BeginSample(Systems[i].SystemName);
                    Systems[i].FixedUpdate();
                    Profiler.EndSample();
                }
                catch (Exception e)
                {
                    SystemsContainer.Get<MainSystem>().HandleException(e, "SystemHandler-FixedUpdate");
                }
            }
        }

        /// <summary>
        /// Call all the updates of the systems
        /// </summary>
        public static void Update()
        {
            for (var i = 0; i < Systems.Length; i++)
            {
                try
                {
                    Profiler.BeginSample(Systems[i].SystemName);
                    Systems[i].Update();
                    Profiler.EndSample();
                }
                catch (Exception e)
                {
                    SystemsContainer.Get<MainSystem>().HandleException(e, "SystemHandler-Update");
                }
            }
        }

        /// <summary>
        /// Call all the updates of the systems
        /// </summary>
        public static void LateUpdate()
        {
            for (var i = 0; i < Systems.Length; i++)
            {
                try
                {
                    Profiler.BeginSample(Systems[i].SystemName);
                    Systems[i].LateUpdate();
                    Profiler.EndSample();
                }
                catch (Exception e)
                {
                    SystemsContainer.Get<MainSystem>().HandleException(e, "SystemHandler-Update");
                }
            }
        }

        /// <summary>
        /// Set all systems as disabled
        /// </summary>
        public static void KillAllSystems()
        {
            for (var i = 0; i < Systems.Length; i++)
            {
                Systems[i].Enabled = false;
            }
        }
    }
}