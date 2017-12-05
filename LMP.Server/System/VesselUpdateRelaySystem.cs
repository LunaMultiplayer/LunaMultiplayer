using LMP.Server.Client;
using LMP.Server.Context;
using LMP.Server.Log;
using LMP.Server.Server;
using LMP.Server.Settings;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace LMP.Server.System
{
    /// <summary>
    /// This class updates the players with the other players vessel updates based on the distance
    /// </summary>
    public class VesselUpdateRelaySystem
    {
        public static ConcurrentQueue<KeyValuePair<ClientStructure, VesselPositionMsgData>> IncomingUpdates { get; } =
            new ConcurrentQueue<KeyValuePair<ClientStructure, VesselPositionMsgData>>();

        private static readonly ConcurrentQueue<KeyValuePair<ClientStructure, VesselPositionMsgData>>
            IncomingMediumUpdates = new ConcurrentQueue<KeyValuePair<ClientStructure, VesselPositionMsgData>>();

        private static readonly ConcurrentQueue<KeyValuePair<ClientStructure, VesselPositionMsgData>> IncomingFarUpdates =
            new ConcurrentQueue<KeyValuePair<ClientStructure, VesselPositionMsgData>>();

        private static readonly ConcurrentDictionary<ClientStructure, VesselPositionMsgData> VesselsDictionary =
            new ConcurrentDictionary<ClientStructure, VesselPositionMsgData>();

        public static void RemovePlayer(ClientStructure client)
        {
            VesselsDictionary.TryRemove(client, out var _);
        }

        public static void AddPlayer(ClientStructure client)
        {
            VesselsDictionary.TryAdd(client, null);
        }

        public static void RelayVesselUpdateMsg(ClientStructure client, VesselPositionMsgData msg)
        {
            IncomingUpdates.Enqueue(new KeyValuePair<ClientStructure, VesselPositionMsgData>(client, msg));
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
                    if (IncomingFarUpdates.TryDequeue(out var vesselUpdate) && VesselsDictionary.ContainsKey(vesselUpdate.Key))
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
                    if (IncomingMediumUpdates.TryDequeue(out var vesselUpdate) && VesselsDictionary.ContainsKey(vesselUpdate.Key))
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
                    if (IncomingUpdates.TryDequeue(out var vesselUpdate) && VesselsDictionary.ContainsKey(vesselUpdate.Key))
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

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static double CalculateDistance(VesselPositionMsgData point1, VesselPositionMsgData point2)
        {
            //return CalculateDistance(new[] { point1.X, point1.Y, point1.Z }, new[] { point2.X, point2.Y, point2.Z });
            return 0;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private static double CalculateDistance(IReadOnlyList<float> point1, IReadOnlyList<float> point2)
        {
            return Math.Sqrt(Math.Pow(point1[0] - point2[0], 2) + Math.Pow(point1[1] - point2[1], 2) + Math.Pow(point1[2] - point2[2], 2));
        }
    }
}
