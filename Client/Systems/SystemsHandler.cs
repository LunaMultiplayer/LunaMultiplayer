using System;
using LunaClient.Base.Interface;
using LunaClient.Systems.Admin;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Chat;
using LunaClient.Systems.ColorSystem;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.Flag;
using LunaClient.Systems.Handshake;
using LunaClient.Systems.KerbalReassigner;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Lock;
using LunaClient.Systems.Mod;
using LunaClient.Systems.ModApi;
using LunaClient.Systems.Motd;
using LunaClient.Systems.Network;
using LunaClient.Systems.PlayerConnection;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Toolbar;
using LunaClient.Systems.VesselChangeSys;
using LunaClient.Systems.VesselDockSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselPositionSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselUpdateSys;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;

namespace LunaClient.Systems
{
    public static class SystemsHandler
    {
        //DO NOT! use a list of ISystem and run trough each method. I don't know what happens but you will 
        //end up with variables that are different than it's singleton value (really, it's a nightmare) 
        //for example, if you stop on TimeSyncerSystem.Update and set Synced = true you will se how 
        //the variable value differs from it's singleton value.
        
        /// <summary>
        /// Call all the updates of the systems
        /// </summary>
        public static void Update()
        {
            TryUpdate(NetworkSystem.Singleton);
            TryUpdate(ModSystem.Singleton);
            TryUpdate(ModApiSystem.Singleton);
            TryUpdate(HandshakeSystem.Singleton);
            TryUpdate(TimeSyncerSystem.Singleton);
            TryUpdate(KerbalSystem.Singleton);
            TryUpdate(VesselLockSystem.Singleton);
            TryUpdate(VesselPositionSystem.Singleton);
            TryUpdate(VesselFlightStateSystem.Singleton);
            TryUpdate(VesselUpdateSystem.Singleton);
            TryUpdate(VesselChangeSystem.Singleton);
            TryUpdate(VesselProtoSystem.Singleton);
            TryUpdate(VesselRemoveSystem.Singleton);
            TryUpdate(VesselDockSystem.Singleton);
            TryUpdate(WarpSystem.Singleton);
            TryUpdate(LockSystem.Singleton);
            TryUpdate(SettingsSystem.Singleton);
            TryUpdate(AsteroidSystem.Singleton);
            TryUpdate(StatusSystem.Singleton);
            TryUpdate(ChatSystem.Singleton);
            TryUpdate(AdminSystem.Singleton);
            TryUpdate(PlayerColorSystem.Singleton);
            TryUpdate(PlayerConnectionSystem.Singleton);
            TryUpdate(MotdSystem.Singleton);
            TryUpdate(CraftLibrarySystem.Singleton);
            TryUpdate(FlagSystem.Singleton);
            TryUpdate(KerbalReassignerSystem.Singleton);
            TryUpdate(ScenarioSystem.Singleton);
            TryUpdate(ToolbarSystem.Singleton);
        }

        /// <summary>
        /// Call all the updates of the systems
        /// </summary>
        public static void LateUpdate()
        {
            TryLateUpdate(NetworkSystem.Singleton);
            TryLateUpdate(ModSystem.Singleton);
            TryLateUpdate(ModApiSystem.Singleton);
            TryLateUpdate(HandshakeSystem.Singleton);
            TryLateUpdate(TimeSyncerSystem.Singleton);
            TryLateUpdate(KerbalSystem.Singleton);
            TryLateUpdate(VesselLockSystem.Singleton);
            TryLateUpdate(VesselPositionSystem.Singleton);
            TryLateUpdate(VesselFlightStateSystem.Singleton);
            TryLateUpdate(VesselUpdateSystem.Singleton);
            TryLateUpdate(VesselChangeSystem.Singleton);
            TryLateUpdate(VesselProtoSystem.Singleton);
            TryLateUpdate(VesselRemoveSystem.Singleton);
            TryLateUpdate(VesselDockSystem.Singleton);
            TryLateUpdate(WarpSystem.Singleton);
            TryLateUpdate(LockSystem.Singleton);
            TryLateUpdate(SettingsSystem.Singleton);
            TryLateUpdate(AsteroidSystem.Singleton);
            TryLateUpdate(StatusSystem.Singleton);
            TryLateUpdate(ChatSystem.Singleton);
            TryLateUpdate(AdminSystem.Singleton);
            TryLateUpdate(PlayerColorSystem.Singleton);
            TryLateUpdate(PlayerConnectionSystem.Singleton);
            TryLateUpdate(MotdSystem.Singleton);
            TryLateUpdate(CraftLibrarySystem.Singleton);
            TryLateUpdate(FlagSystem.Singleton);
            TryLateUpdate(KerbalReassignerSystem.Singleton);
            TryLateUpdate(ScenarioSystem.Singleton);
            TryLateUpdate(ToolbarSystem.Singleton);
        }

        /// <summary>
        /// Call all the fixed updates of the systems
        /// </summary>
        public static void FixedUpdate()
        {
            int LOGGING_INTERVAL = 100;
            int NEVER_LOG_AVERAGES = 0;
            Timer.Start("FixedUpdate", LOGGING_INTERVAL);
            Timer.Start("NetworkSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(NetworkSystem.Singleton);
            Timer.Stop("NetworkSystem");
            Timer.Start("ModSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(ModSystem.Singleton);
            Timer.Stop("ModSystem");
            Timer.Start("ModApiSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(ModApiSystem.Singleton);
            Timer.Stop("ModApiSystem");
            Timer.Start("HandshakeSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(HandshakeSystem.Singleton);
            Timer.Stop("HandshakeSystem");
            Timer.Start("TimeSyncerSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(TimeSyncerSystem.Singleton);
            Timer.Stop("TimeSyncerSystem");
            Timer.Start("KerbalSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(KerbalSystem.Singleton);
            Timer.Stop("KerbalSystem");
            Timer.Start("VesselLockSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(VesselLockSystem.Singleton);
            Timer.Stop("VesselLockSystem");
            Timer.Start("VesselPositionSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(VesselPositionSystem.Singleton);
            Timer.Stop("VesselPositionSystem");
            Timer.Start("VesselFlightStateSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(VesselFlightStateSystem.Singleton);
            Timer.Stop("VesselFlightStateSystem");
            Timer.Start("VesselUpdateSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(VesselUpdateSystem.Singleton);
            Timer.Stop("VesselUpdateSystem");
            TryFixedUpdate(VesselChangeSystem.Singleton);
            Timer.Start("VesselProtoSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(VesselProtoSystem.Singleton);
            Timer.Stop("VesselProtoSystem");
            Timer.Start("VesselRemoveSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(VesselRemoveSystem.Singleton);
            Timer.Stop("VesselRemoveSystem");
            Timer.Start("VesselDockSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(VesselDockSystem.Singleton);
            Timer.Stop("VesselDockSystem");
            Timer.Start("WarpSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(WarpSystem.Singleton);
            Timer.Stop("WarpSystem");
            Timer.Start("LockSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(LockSystem.Singleton);
            Timer.Stop("LockSystem");
            Timer.Start("SettingsSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(SettingsSystem.Singleton);
            Timer.Stop("SettingsSystem");
            Timer.Start("AsteroidSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(AsteroidSystem.Singleton);
            Timer.Stop("AsteroidSystem");
            Timer.Start("StatusSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(StatusSystem.Singleton);
            Timer.Stop("StatusSystem");
            Timer.Start("ChatSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(ChatSystem.Singleton);
            Timer.Stop("ChatSystem");
            Timer.Start("AdminSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(AdminSystem.Singleton);
            Timer.Stop("AdminSystem");
            Timer.Start("PlayerColorSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(PlayerColorSystem.Singleton);
            Timer.Stop("PlayerColorSystem");
            Timer.Start("PlayerConnectionSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(PlayerConnectionSystem.Singleton);
            Timer.Stop("PlayerConnectionSystem");
            Timer.Start("MotdSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(MotdSystem.Singleton);
            Timer.Stop("MotdSystem");
            Timer.Start("CraftLibrarySystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(CraftLibrarySystem.Singleton);
            Timer.Stop("CraftLibrarySystem");
            Timer.Start("FlagSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(FlagSystem.Singleton);
            Timer.Stop("FlagSystem");
            Timer.Start("KerbalReassignerSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(KerbalReassignerSystem.Singleton);
            Timer.Stop("KerbalReassignerSystem");
            Timer.Start("ScenarioSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(ScenarioSystem.Singleton);
            Timer.Stop("ScenarioSystem");
            Timer.Start("ToolbarSystem", NEVER_LOG_AVERAGES);
            TryFixedUpdate(ToolbarSystem.Singleton);
            Timer.Stop("ToolbarSystem");
            Timer.Stop("FixedUpdate");
        }

        private static void TryUpdate(ISystem system)
        {
            try
            {
                system.Update();
            }
            catch (Exception e)
            {
                MainSystem.Singleton.HandleException(e, "SystemHandler-Update");
            }
        }

        private static void TryLateUpdate(ISystem system)
        {
            try
            {
                system.LateUpdate();
            }
            catch (Exception e)
            {
                MainSystem.Singleton.HandleException(e, "SystemHandler-LateUpdate");
            }
        }

        private static void TryFixedUpdate(ISystem system)
        {
            try
            {
                system.FixedUpdate();
            }
            catch (Exception e)
            {
                MainSystem.Singleton.HandleException(e, "SystemHandler-FixedUpdate");
            }
        }

        public static void KillAllSystems()
        {
            VesselCommon.EnableAllSystems = false;
            NetworkSystem.Singleton.Enabled = false;
            ModSystem.Singleton.Enabled = false;
            ModApiSystem.Singleton.Enabled = false;
            SettingsSystem.Singleton.Enabled = false;
            ToolbarSystem.Singleton.Enabled = false;
            HandshakeSystem.Singleton.Enabled = false;
            TimeSyncerSystem.Singleton.Enabled = false;
            KerbalSystem.Singleton.Enabled = false;
            WarpSystem.Singleton.Enabled = false;
            LockSystem.Singleton.Enabled = false;
            AsteroidSystem.Singleton.Enabled = false;
            StatusSystem.Singleton.Enabled = false;
            ChatSystem.Singleton.Enabled = false;
            AdminSystem.Singleton.Enabled = false;
            PlayerColorSystem.Singleton.Enabled = false;
            PlayerConnectionSystem.Singleton.Enabled = false;
            MotdSystem.Singleton.Enabled = false;
            CraftLibrarySystem.Singleton.Enabled = false;
            FlagSystem.Singleton.Enabled = false;
            KerbalReassignerSystem.Singleton.Enabled = false;
            ScenarioSystem.Singleton.Enabled = false;
        }
    }
}