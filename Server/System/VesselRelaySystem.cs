using LunaCommon.Message.Data.Vessel;
using LunaServer.Client;
using LunaServer.Enums;
using LunaServer.Settings;
using System;

namespace LunaServer.System
{
    /// <summary>
    /// This class relay the vessel messages to the correct subspaces
    /// Based on a setting we either call the DatabaseRelaySystem or the DictionaryRelaySystem
    /// </summary>
    public static class VesselRelaySystem
    {        
        /// <summary>
        /// This method relays a message to the other clients in the same subspace.
        /// In case there are other players in OLDER subspaces it stores it in their queue for further processing
        /// </summary>
        public static void HandleVesselMessage(ClientStructure client, VesselBaseMsgData msg)
        {
            switch (GeneralSettings.SettingsStore.RelaySystemMode)
            {
                case RelaySystemMode.Dictionary:
                    VesselRelaySystemDictionary.HandleVesselMessage(client, msg);
                    break;
                case RelaySystemMode.Database:
                    VesselRelaySystemDataBase.HandleVesselMessage(client, msg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Creates a new subspace and sets its message queue from a past subspace.
        /// Must be called AFTER the subspace is created in the warp context.
        /// </summary>
        public static void CreateNewSubspace(int subspaceId)
        {
            switch (GeneralSettings.SettingsStore.RelaySystemMode)
            {
                case RelaySystemMode.Dictionary:
                    VesselRelaySystemDictionary.CreateNewSubspace(subspaceId);
                    break;
                case RelaySystemMode.Database:
                    VesselRelaySystemDataBase.CreateNewSubspace(subspaceId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Removes a subspace
        /// </summary>
        public static void RemoveSubspace(int subspaceId)
        {
            switch (GeneralSettings.SettingsStore.RelaySystemMode)
            {
                case RelaySystemMode.Dictionary:
                    VesselRelaySystemDictionary.RemoveSubspace(subspaceId);
                    break;
                case RelaySystemMode.Database:
                    VesselRelaySystemDataBase.RemoveSubspace(subspaceId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// This method should be called in a thread. 
        /// It runs over the old messages and sends them once the subspace time matches the message send time.
        /// </summary>
        public static void RelayOldVesselMessages()
        {
            switch (GeneralSettings.SettingsStore.RelaySystemMode)
            {
                case RelaySystemMode.Dictionary:
                    VesselRelaySystemDictionary.RelayOldVesselMessages();
                    break;
                case RelaySystemMode.Database:
                    VesselRelaySystemDataBase.RelayOldVesselMessages();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
