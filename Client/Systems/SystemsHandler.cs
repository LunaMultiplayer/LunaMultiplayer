using System;
using LunaClient.Base.Interface;
using LunaClient.Systems.Admin;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.AtmoLoader;
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
using LunaClient.Systems.PartKiller;
using LunaClient.Systems.PlayerConnection;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Toolbar;
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
        /// Clear the received message queues of all the systems that handle messages
        /// </summary>
        public static void ClearMessageQueues()
        {
            ModApiSystem.Singleton.ClearIncomingMsgQueue();
            HandshakeSystem.Singleton.ClearIncomingMsgQueue();
            TimeSyncerSystem.Singleton.ClearIncomingMsgQueue();
            KerbalSystem.Singleton.ClearIncomingMsgQueue();
            VesselUpdateSystem.Singleton.ClearIncomingMsgQueue();
            VesselProtoSystem.Singleton.ClearIncomingMsgQueue();
            VesselRemoveSystem.Singleton.ClearIncomingMsgQueue();
            WarpSystem.Singleton.ClearIncomingMsgQueue();
            LockSystem.Singleton.ClearIncomingMsgQueue();
            SettingsSystem.Singleton.ClearIncomingMsgQueue();
            StatusSystem.Singleton.ClearIncomingMsgQueue();
            ChatSystem.Singleton.ClearIncomingMsgQueue();
            AdminSystem.Singleton.ClearIncomingMsgQueue();
            PlayerColorSystem.Singleton.ClearIncomingMsgQueue();
            PlayerConnectionSystem.Singleton.ClearIncomingMsgQueue();
            MotdSystem.Singleton.ClearIncomingMsgQueue();
            CraftLibrarySystem.Singleton.ClearIncomingMsgQueue();
            FlagSystem.Singleton.ClearIncomingMsgQueue();
            ScenarioSystem.Singleton.ClearIncomingMsgQueue();
            HandshakeSystem.Singleton.ClearIncomingMsgQueue();
        }

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
            TryUpdate(VesselProtoSystem.Singleton);
            TryUpdate(VesselRemoveSystem.Singleton);
            TryUpdate(VesselWarpSystem.Singleton);
            TryUpdate(VesselDockSystem.Singleton);
            TryUpdate(WarpSystem.Singleton);
            TryUpdate(LockSystem.Singleton);
            TryUpdate(SettingsSystem.Singleton);
            TryUpdate(AtmoLoaderSystem.Singleton);
            TryUpdate(PartKillerSystem.Singleton);
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
            TryLateUpdate(VesselProtoSystem.Singleton);
            TryLateUpdate(VesselRemoveSystem.Singleton);
            TryLateUpdate(VesselWarpSystem.Singleton);
            TryLateUpdate(VesselDockSystem.Singleton);
            TryLateUpdate(WarpSystem.Singleton);
            TryLateUpdate(LockSystem.Singleton);
            TryLateUpdate(SettingsSystem.Singleton);
            TryLateUpdate(AtmoLoaderSystem.Singleton);
            TryLateUpdate(PartKillerSystem.Singleton);
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
            TryFixedUpdate(VesselProtoSystem.Singleton);
            TryFixedUpdate(VesselRemoveSystem.Singleton);
            TryFixedUpdate(VesselWarpSystem.Singleton);
            TryFixedUpdate(VesselDockSystem.Singleton);
            TryFixedUpdate(WarpSystem.Singleton);
            TryFixedUpdate(LockSystem.Singleton);
            TryFixedUpdate(SettingsSystem.Singleton);
            TryFixedUpdate(AtmoLoaderSystem.Singleton);
            TryFixedUpdate(PartKillerSystem.Singleton);
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
        /// Call all the resets of the systems
        /// </summary>
        public static void Reset(bool resetMainSystems = true)
        {
            if (resetMainSystems)
            {
                TryReset(NetworkSystem.Singleton);
                TryReset(ModSystem.Singleton);
                TryReset(ModApiSystem.Singleton);
                TryReset(SettingsSystem.Singleton);
                TryReset(ToolbarSystem.Singleton);
            }
            TryReset(HandshakeSystem.Singleton);
            TryReset(TimeSyncerSystem.Singleton);
            TryReset(KerbalSystem.Singleton);
            TryReset(VesselUpdateSystem.Singleton);
            TryReset(VesselLockSystem.Singleton);
            TryReset(VesselProtoSystem.Singleton);
            TryReset(VesselRemoveSystem.Singleton);
            TryReset(VesselWarpSystem.Singleton);
            TryReset(VesselDockSystem.Singleton);
            TryReset(WarpSystem.Singleton);
            TryReset(LockSystem.Singleton);
            TryReset(AtmoLoaderSystem.Singleton);
            TryReset(PartKillerSystem.Singleton);
            TryReset(AsteroidSystem.Singleton);
            TryReset(StatusSystem.Singleton);
            TryReset(ChatSystem.Singleton);
            TryReset(AdminSystem.Singleton);
            TryReset(PlayerColorSystem.Singleton);
            TryReset(PlayerConnectionSystem.Singleton);
            TryReset(MotdSystem.Singleton);
            TryReset(CraftLibrarySystem.Singleton);
            TryReset(FlagSystem.Singleton);
            TryReset(KerbalReassignerSystem.Singleton);
            TryReset(ScenarioSystem.Singleton);
        }

        /// <summary>
        /// When connecting, set the systems to false
        /// </summary>
        public static void DisableSystemsOnConnecting()
        {
            AsteroidSystem.Singleton.Enabled = false;
            VesselCommon.EnableAllSystems = false;
            AtmoLoaderSystem.Singleton.Enabled = false;
            StatusSystem.Singleton.Enabled = false;
            ScenarioSystem.Singleton.Enabled = false;
            WarpSystem.Singleton.Enabled = false;
            CraftLibrarySystem.Singleton.Enabled = false;
            TimeSyncerSystem.Singleton.Enabled = false;
            AdminSystem.Singleton.Enabled = false;
            ChatSystem.Singleton.Enabled = false;
            PlayerColorSystem.Singleton.Enabled = false;
            PlayerConnectionSystem.Singleton.Enabled = false;
            MotdSystem.Singleton.Enabled = false;
            FlagSystem.Singleton.Enabled = false;
            PartKillerSystem.Singleton.Enabled = false;
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

        private static void TryReset(ISystem system)
        {
            try
            {
                system.Reset();
            }
            catch (Exception e)
            {
                MainSystem.Singleton.HandleException(e, "SystemHandler-Reset");
            }
        }
    }
}