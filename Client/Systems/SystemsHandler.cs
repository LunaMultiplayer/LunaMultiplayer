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
using LunaClient.Systems.VesselWarpSys;
using LunaClient.Systems.Warp;

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
            TryUpdate(VesselWarpSystem.Singleton);
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
            TryLateUpdate(VesselWarpSystem.Singleton);
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
            TryFixedUpdate(NetworkSystem.Singleton);
            TryFixedUpdate(ModSystem.Singleton);
            TryFixedUpdate(ModApiSystem.Singleton);
            TryFixedUpdate(HandshakeSystem.Singleton);
            TryFixedUpdate(TimeSyncerSystem.Singleton);
            TryFixedUpdate(KerbalSystem.Singleton);
            TryFixedUpdate(VesselLockSystem.Singleton);
            TryFixedUpdate(VesselUpdateSystem.Singleton);
            TryFixedUpdate(VesselChangeSystem.Singleton);
            TryFixedUpdate(VesselProtoSystem.Singleton);
            TryFixedUpdate(VesselRemoveSystem.Singleton);
            TryFixedUpdate(VesselWarpSystem.Singleton);
            TryFixedUpdate(VesselDockSystem.Singleton);
            TryFixedUpdate(WarpSystem.Singleton);
            TryFixedUpdate(LockSystem.Singleton);
            TryFixedUpdate(SettingsSystem.Singleton);
            TryFixedUpdate(AsteroidSystem.Singleton);
            TryFixedUpdate(StatusSystem.Singleton);
            TryFixedUpdate(ChatSystem.Singleton);
            TryFixedUpdate(AdminSystem.Singleton);
            TryFixedUpdate(PlayerColorSystem.Singleton);
            TryFixedUpdate(PlayerConnectionSystem.Singleton);
            TryFixedUpdate(MotdSystem.Singleton);
            TryFixedUpdate(CraftLibrarySystem.Singleton);
            TryFixedUpdate(FlagSystem.Singleton);
            TryFixedUpdate(KerbalReassignerSystem.Singleton);
            TryFixedUpdate(ScenarioSystem.Singleton);
            TryFixedUpdate(ToolbarSystem.Singleton);
        }
        
        /// <summary>
        /// When connecting, set the systems to false
        /// </summary>
        public static void DisableSystemsOnConnecting()
        {
            SettingsSystem.Singleton.Enabled = false;
            LockSystem.Singleton.Enabled = false;
            KerbalSystem.Singleton.Enabled = false;
            HandshakeSystem.Singleton.Enabled = false;
            AdminSystem.Singleton.Enabled = false;
            AsteroidSystem.Singleton.Enabled = false;
            VesselCommon.EnableAllSystems = false;
            StatusSystem.Singleton.Enabled = false;
            ScenarioSystem.Singleton.Enabled = false;
            WarpSystem.Singleton.Enabled = false;
            CraftLibrarySystem.Singleton.Enabled = false;
            TimeSyncerSystem.Singleton.Enabled = false;
            ChatSystem.Singleton.Enabled = false;
            PlayerColorSystem.Singleton.Enabled = false;
            PlayerConnectionSystem.Singleton.Enabled = false;
            MotdSystem.Singleton.Enabled = false;
            FlagSystem.Singleton.Enabled = false;
            KerbalReassignerSystem.Singleton.Enabled = false;
            VesselWarpSystem.Singleton.Enabled = false;
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

        public static void Reset()
        {
            NetworkSystem.Singleton.Enabled = false;
            ModSystem.Singleton.Enabled = false;
            ModApiSystem.Singleton.Enabled = false;
            SettingsSystem.Singleton.Enabled = false;
            ToolbarSystem.Singleton.Enabled = false;
            HandshakeSystem.Singleton.Enabled = false;
            TimeSyncerSystem.Singleton.Enabled = false;
            KerbalSystem.Singleton.Enabled = false;
            VesselUpdateSystem.Singleton.Enabled = false;
            VesselLockSystem.Singleton.Enabled = false;
            VesselChangeSystem.Singleton.Enabled = false;
            VesselProtoSystem.Singleton.Enabled = false;
            VesselRemoveSystem.Singleton.Enabled = false;
            VesselWarpSystem.Singleton.Enabled = false;
            VesselDockSystem.Singleton.Enabled = false;
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

            NetworkSystem.Singleton.Enabled = true;
            ModSystem.Singleton.Enabled = true;
            ModApiSystem.Singleton.Enabled = true;
            SettingsSystem.Singleton.Enabled = true;
            ToolbarSystem.Singleton.Enabled = true;
            HandshakeSystem.Singleton.Enabled = true;
            TimeSyncerSystem.Singleton.Enabled = true;
            KerbalSystem.Singleton.Enabled = true;
            VesselUpdateSystem.Singleton.Enabled = true;
            VesselLockSystem.Singleton.Enabled = true;
            VesselChangeSystem.Singleton.Enabled = true;
            VesselProtoSystem.Singleton.Enabled = true;
            VesselRemoveSystem.Singleton.Enabled = true;
            VesselWarpSystem.Singleton.Enabled = true;
            VesselDockSystem.Singleton.Enabled = true;
            WarpSystem.Singleton.Enabled = true;
            LockSystem.Singleton.Enabled = true;
            AsteroidSystem.Singleton.Enabled = true;
            StatusSystem.Singleton.Enabled = true;
            ChatSystem.Singleton.Enabled = true;
            AdminSystem.Singleton.Enabled = true;
            PlayerColorSystem.Singleton.Enabled = true;
            PlayerConnectionSystem.Singleton.Enabled = true;
            MotdSystem.Singleton.Enabled = true;
            CraftLibrarySystem.Singleton.Enabled = true;
            FlagSystem.Singleton.Enabled = true;
            KerbalReassignerSystem.Singleton.Enabled = true;
            ScenarioSystem.Singleton.Enabled = true;
        }
    }
}