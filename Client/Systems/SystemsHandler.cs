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
using LunaClient.Systems.VesselLockSys;
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
            Timer.Start("FixedUpdate", LOGGING_INTERVAL);
            Timer.Start("NetworkSystem", LOGGING_INTERVAL);
            TryFixedUpdate(NetworkSystem.Singleton);
            Timer.Stop("NetworkSystem");
            Timer.Start("ModSystem", LOGGING_INTERVAL);
            TryFixedUpdate(ModSystem.Singleton);
            Timer.Stop("ModSystem");
            Timer.Start("ModApiSystem", LOGGING_INTERVAL);
            TryFixedUpdate(ModApiSystem.Singleton);
            Timer.Stop("ModApiSystem");
            Timer.Start("HandshakeSystem", LOGGING_INTERVAL);
            TryFixedUpdate(HandshakeSystem.Singleton);
            Timer.Stop("HandshakeSystem");
            Timer.Start("TimeSyncerSystem", LOGGING_INTERVAL);
            TryFixedUpdate(TimeSyncerSystem.Singleton);
            Timer.Stop("TimeSyncerSystem");
            Timer.Start("KerbalSystem", LOGGING_INTERVAL);
            TryFixedUpdate(KerbalSystem.Singleton);
            Timer.Stop("KerbalSystem");
            Timer.Start("VesselLockSystem", LOGGING_INTERVAL);
            TryFixedUpdate(VesselLockSystem.Singleton);
            Timer.Stop("VesselLockSystem");
            Timer.Start("VesselUpdateSystem", LOGGING_INTERVAL);
            TryFixedUpdate(VesselUpdateSystem.Singleton);
            Timer.Stop("VesselUpdateSystem");
            TryFixedUpdate(VesselChangeSystem.Singleton);
            Timer.Start("VesselProtoSystem", LOGGING_INTERVAL);
            TryFixedUpdate(VesselProtoSystem.Singleton);
            Timer.Stop("VesselProtoSystem");
            Timer.Start("VesselRemoveSystem", LOGGING_INTERVAL);
            TryFixedUpdate(VesselRemoveSystem.Singleton);
            Timer.Stop("VesselRemoveSystem");
            Timer.Start("VesselDockSystem", LOGGING_INTERVAL);
            TryFixedUpdate(VesselDockSystem.Singleton);
            Timer.Stop("VesselDockSystem");
            Timer.Start("WarpSystem", LOGGING_INTERVAL);
            TryFixedUpdate(WarpSystem.Singleton);
            Timer.Stop("WarpSystem");
            Timer.Start("LockSystem", LOGGING_INTERVAL);
            TryFixedUpdate(LockSystem.Singleton);
            Timer.Stop("LockSystem");
            Timer.Start("SettingsSystem", LOGGING_INTERVAL);
            TryFixedUpdate(SettingsSystem.Singleton);
            Timer.Stop("SettingsSystem");
            Timer.Start("AsteroidSystem", LOGGING_INTERVAL);
            TryFixedUpdate(AsteroidSystem.Singleton);
            Timer.Stop("AsteroidSystem");
            Timer.Start("StatusSystem", LOGGING_INTERVAL);
            TryFixedUpdate(StatusSystem.Singleton);
            Timer.Stop("StatusSystem");
            Timer.Start("ChatSystem", LOGGING_INTERVAL);
            TryFixedUpdate(ChatSystem.Singleton);
            Timer.Stop("ChatSystem");
            Timer.Start("AdminSystem", LOGGING_INTERVAL);
            TryFixedUpdate(AdminSystem.Singleton);
            Timer.Stop("AdminSystem");
            Timer.Start("PlayerColorSystem", LOGGING_INTERVAL);
            TryFixedUpdate(PlayerColorSystem.Singleton);
            Timer.Stop("PlayerColorSystem");
            Timer.Start("PlayerConnectionSystem", LOGGING_INTERVAL);
            TryFixedUpdate(PlayerConnectionSystem.Singleton);
            Timer.Stop("PlayerConnectionSystem");
            Timer.Start("MotdSystem", LOGGING_INTERVAL);
            TryFixedUpdate(MotdSystem.Singleton);
            Timer.Stop("MotdSystem");
            Timer.Start("CraftLibrarySystem", LOGGING_INTERVAL);
            TryFixedUpdate(CraftLibrarySystem.Singleton);
            Timer.Stop("CraftLibrarySystem");
            Timer.Start("FlagSystem", LOGGING_INTERVAL);
            TryFixedUpdate(FlagSystem.Singleton);
            Timer.Stop("FlagSystem");
            Timer.Start("KerbalReassignerSystem", LOGGING_INTERVAL);
            TryFixedUpdate(KerbalReassignerSystem.Singleton);
            Timer.Stop("KerbalReassignerSystem");
            Timer.Start("ScenarioSystem", LOGGING_INTERVAL);
            TryFixedUpdate(ScenarioSystem.Singleton);
            Timer.Stop("ScenarioSystem");
            Timer.Start("ToolbarSystem", LOGGING_INTERVAL);
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