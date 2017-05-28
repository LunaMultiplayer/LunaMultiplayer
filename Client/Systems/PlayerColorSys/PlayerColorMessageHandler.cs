using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Lock;
using LunaClient.Windows.Status;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Color;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;
using System.Collections.Concurrent;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.PlayerColorSys
{
    public class PlayerColorMessageHandler : SubSystem<PlayerColorSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as PlayerColorBaseMsgData;
            if (msgData == null) return;

            switch (msgData.PlayerColorMessageType)
            {
                case PlayerColorMessageType.Reply:
                    {
                        var data = (PlayerColorReplyMsgData)messageData;
                        System.PlayerColors.Clear();
                        for (var i = 0; i < data.Count; i++)
                        {
                            var playerName = data.PlayersColors[i].Key;
                            var playerColor = System.ConvertStringToColor(data.PlayersColors[i].Value);
                            System.PlayerColors.Add(playerName, playerColor);

                            StatusWindow.Singleton.ColorEventHandled = false; //Refresh colors in status window
                        }
                        MainSystem.Singleton.NetworkState = ClientState.ColorsSynced;
                    }
                    break;
                case PlayerColorMessageType.Set:
                    {
                        //Player joined or changed it's color so update his controlled vessel orbit colors
                        var data = (PlayerColorSetMsgData)messageData;
                        var playerName = data.PlayerName;
                        var playerColor = System.ConvertStringToColor(data.Color);
                        Debug.Log($"[LMP]: Color Message, Name: {playerName} , color: {playerColor}");
                        System.PlayerColors[playerName] = playerColor;
                        UpdateVesselColors(playerName);

                        StatusWindow.Singleton.ColorEventHandled = false;//Refresh colors in status window
                    }
                    break;
            }
        }

        /// <summary>
        /// Update the vessel colors of the given player
        /// </summary>
        public void UpdateVesselColors(string playerName)
        {
            var controlledVesselIds = LockSystem.Singleton.GetPlayerLocks(playerName)
                .Where(l => l.StartsWith("control-"))
                .Select(l => l.Substring(8)).ToArray();

            foreach (var vesselId in controlledVesselIds)
            {
                var vesselToUpdate = FlightGlobals.FindVessel(new Guid(vesselId));
                if (vesselToUpdate != null)
                {
                    System.PlayerColorEvents.SetVesselOrbitColor(vesselToUpdate);
                }
            }
        }
    }
}