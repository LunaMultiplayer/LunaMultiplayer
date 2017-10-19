using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Lock;
using LunaClient.Windows;
using LunaClient.Windows.Status;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Color;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;
using UniLinq;

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

                            WindowsContainer.Get<StatusWindow>().ColorEventHandled = false; //Refresh colors in status window
                        }
                        MainSystem.NetworkState = ClientState.ColorsSynced;
                    }
                    break;
                case PlayerColorMessageType.Set:
                    {
                        //Player joined or changed it's color so update his controlled vessel orbit colors
                        var data = (PlayerColorSetMsgData)messageData;
                        var playerName = data.PlayerName;
                        var playerColor = System.ConvertStringToColor(data.Color);
                        LunaLog.Log($"[LMP]: Color Message, Name: {playerName} , color: {playerColor}");
                        System.PlayerColors[playerName] = playerColor;
                        UpdateVesselColors(playerName);

                        WindowsContainer.Get<StatusWindow>().ColorEventHandled = false;//Refresh colors in status window
                    }
                    break;
            }
        }

        /// <summary>
        /// Update the vessel colors of the given player
        /// </summary>
        public void UpdateVesselColors(string playerName)
        {
            var controlledVesselIds = LockSystem.LockQuery.GetAllControlLocks(playerName)
                .Select(l => l.VesselId).ToArray();

            foreach (var vesselId in controlledVesselIds)
            {
                var vesselToUpdate = FlightGlobals.FindVessel(vesselId);
                if (vesselToUpdate != null)
                {
                    System.PlayerColorEvents.SetVesselOrbitColor(vesselToUpdate);
                }
            }
        }
    }
}