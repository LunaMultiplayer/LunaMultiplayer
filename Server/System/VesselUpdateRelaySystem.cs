using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Server;
using LunaServer.Settings;

namespace LunaServer.System
{
    /// <summary>
    /// This class updates the players with the other players vessel updates based on the distance
    /// </summary>
    public class VesselUpdateRelaySystem
    {
        public static ConcurrentQueue<KeyValuePair<ClientStructure, VesselUpdateMsgData>> IncomingUpdates { get; }=
            new ConcurrentQueue<KeyValuePair<ClientStructure, VesselUpdateMsgData>>();

        private static readonly ConcurrentQueue<KeyValuePair<ClientStructure, VesselUpdateMsgData>>
            IncomingMediumUpdates = new ConcurrentQueue<KeyValuePair<ClientStructure, VesselUpdateMsgData>>();

        private static readonly ConcurrentQueue<KeyValuePair<ClientStructure, VesselUpdateMsgData>> IncomingFarUpdates =
            new ConcurrentQueue<KeyValuePair<ClientStructure, VesselUpdateMsgData>>();

        private static readonly ConcurrentDictionary<ClientStructure, VesselUpdateMsgData> VesselsDictionary =
            new ConcurrentDictionary<ClientStructure, VesselUpdateMsgData>();

        public static void RemovePlayer(ClientStructure client)
        {
            VesselUpdateMsgData value;
            VesselsDictionary.TryRemove(client, out value);
        }

        public static void AddPlayer(ClientStructure client)
        {
            VesselsDictionary.TryAdd(client, null);
        }

        public static void RelayVesselUpdateMsg(ClientStructure client, VesselUpdateMsgData msg)
        {
            IncomingUpdates.Enqueue(new KeyValuePair<ClientStructure, VesselUpdateMsgData>(client, msg));
        }

        /// <summary>
        /// This method relay the vessel update to players in other planets
        /// </summary>
        public static void RelayToFarPlayers()
        {
            while (ServerContext.ServerRunning)
            {
                try
                {
                    KeyValuePair<ClientStructure, VesselUpdateMsgData> vesselUpdate;
                    if (IncomingFarUpdates.TryDequeue(out vesselUpdate) && VesselsDictionary.ContainsKey(vesselUpdate.Key))
                    {
                        var farClients = VesselsDictionary.Where(v => !Equals(v.Key, vesselUpdate.Key) && v.Value != null &&
                                                                      v.Value.BodyName != vesselUpdate.Value.BodyName)
                            .Select(v => v.Key);

                        foreach (var farClient in farClients.Where(c => ServerContext.Clients.ContainsKey(c.Endpoint)))
                            MessageQueuer.RelayMessage<VesselSrvMsg>(farClient, vesselUpdate.Value);
                    }
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Error in RelayToFarPlayers method! Details: {e}");
                }
                Thread.Sleep(GeneralSettings.SettingsStore.FarDistanceUpdateIntervalMs);
            }
        }

        /// <summary>
        /// This method relay the update to players in the same planet but farther than 25km
        /// </summary>
        public static void RelayToMediumDistancePlayers()
        {
            while (ServerContext.ServerRunning)
            {
                try
                {
                    KeyValuePair<ClientStructure, VesselUpdateMsgData> vesselUpdate;
                    if (IncomingMediumUpdates.TryDequeue(out vesselUpdate) && VesselsDictionary.ContainsKey(vesselUpdate.Key))
                    {
                        IncomingFarUpdates.Enqueue(vesselUpdate);

                        var mediumDistanceClients = VesselsDictionary.Where(
                                v => !Equals(v.Key, vesselUpdate.Key) && v.Value != null &&
                                     v.Value.BodyName == vesselUpdate.Value.BodyName &&
                                     CalculateDistance(v.Value, vesselUpdate.Value) >
                                     GeneralSettings.SettingsStore.CloseDistanceInMeters)
                            .Select(v => v.Key);

                        foreach (
                            var mediumDistanceClient in
                            mediumDistanceClients.Where(c => ServerContext.Clients.ContainsKey(c.Endpoint)))
                            MessageQueuer.RelayMessage<VesselSrvMsg>(mediumDistanceClient, vesselUpdate.Value);
                    }
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Error in RelayToMediumDistancePlayers method! Details: {e}");
                }
                Thread.Sleep(GeneralSettings.SettingsStore.MediumDistanceUpdateIntervalMs);
            }
        }

        /// <summary>
        /// This method relay the update to players closer than 25km
        /// </summary>
        public static void RelayToClosePlayers()
        {
            while (ServerContext.ServerRunning)
            {
                try
                {
                    KeyValuePair<ClientStructure, VesselUpdateMsgData> vesselUpdate;
                    if (IncomingUpdates.TryDequeue(out vesselUpdate) && VesselsDictionary.ContainsKey(vesselUpdate.Key))
                    {
                        VesselsDictionary.TryUpdate(vesselUpdate.Key, vesselUpdate.Value, VesselsDictionary[vesselUpdate.Key]);

                        IncomingMediumUpdates.Enqueue(vesselUpdate);

                        var closeClients = VesselsDictionary.Where(v => !Equals(v.Key, vesselUpdate.Key) && v.Value != null &&
                                                                        v.Value.BodyName == vesselUpdate.Value.BodyName &&
                                                                        CalculateDistance(v.Value, vesselUpdate.Value) <=
                                                                        GeneralSettings.SettingsStore.CloseDistanceInMeters)
                            .Select(v => v.Key);

                        foreach (var closeClient in closeClients.Where(c => ServerContext.Clients.ContainsKey(c.Endpoint)))
                            MessageQueuer.RelayMessage<VesselSrvMsg>(closeClient, vesselUpdate.Value);
                    }
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Error in RelayToClosePlayers method! Details: {e}");
                }
                Thread.Sleep(GeneralSettings.SettingsStore.CloseDistanceUpdateIntervalMs);
            }
        }

        private static double CalculateDistance(VesselUpdateMsgData point1, VesselUpdateMsgData point2)
        {
            return CalculateDistance(new[] { point1.X, point1.Y, point1.Z }, new[] { point2.X, point2.Y, point2.Z });
        }

        private static double CalculateDistance(IReadOnlyList<float> point1, IReadOnlyList<float> point2)
        {
            return Math.Sqrt(Math.Pow(point1[0] - point2[0], 2) + Math.Pow(point1[1] - point2[1], 2) + Math.Pow(point1[2] - point2[2], 2));
        }
    }
}
