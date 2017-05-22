using System;
using System.Collections.Generic;
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
using LunaClient.Systems.Warp;

namespace LunaClient.Systems
{
    public static class SystemsHandler
    {
        //DO NOT! use a list of ISystem and run trough each method. I don't know what happens but you will 
        //end up with variables that are different than it's singleton value (really, it's a nightmare) 
        //for example, if you stop on TimeSyncerSystem.Update and set Synced = true you will see how 
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
            TryUpdate(VesselCommon.GetSingletons);
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
            TryFixedUpdate(VesselCommon.GetSingletons);
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

        private static void TryUpdate(params ISystem[] systems)
        {
            foreach (var system in systems)
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
        }
        
        private static void TryFixedUpdate(params ISystem[] systems)
        {
            foreach (var system in systems)
            {
                try
                {
                    system.RunFixedUpdate();
                }
                catch (Exception e)
                {
                    MainSystem.Singleton.HandleException(e, "SystemHandler-FixedUpdate");
                }
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