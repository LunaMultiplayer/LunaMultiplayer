using System.Collections.Concurrent;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Extensions;
using LmpClient.Systems.Lock;
using LmpClient.Windows.Status;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Color;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using UniLinq;

namespace LmpClient.Systems.PlayerColorSys
{
    public class PlayerColorMessageHandler : SubSystem<PlayerColorSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is PlayerColorBaseMsgData msgData)) return;

            switch (msgData.PlayerColorMessageType)
            {
                case PlayerColorMessageType.Reply:
                    {
                        var data = (PlayerColorReplyMsgData)msgData;
                        System.PlayerColors.Clear();
                        for (var i = 0; i < data.PlayerColorsCount; i++)
                        {
                            var playerName = data.PlayersColors[i].PlayerName;
                            System.PlayerColors.Add(playerName, data.PlayersColors[i].Color);

                            StatusWindow.Singleton.ColorEventHandled = false; //Refresh colors in status window
                        }
                        MainSystem.NetworkState = ClientState.ColorsSynced;
                    }
                    break;
                case PlayerColorMessageType.Set:
                    {
                        //Player joined or changed it's color so update his controlled vessel orbit colors
                        var data = (PlayerColorSetMsgData)msgData;
                        var playerName = data.PlayerColor.PlayerName;
                        var playerColor = data.PlayerColor.Color;

                        LunaLog.Log($"[LMP]: Color Message, Name: {playerName} , color: {playerColor}");
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
            var controlledVesselIds = LockSystem.LockQuery.GetAllControlLocks(playerName)
                .Select(l => FlightGlobals.FindVessel(l.VesselId))
                .Where(v => v != null);

            foreach (var vessel in controlledVesselIds)
            {
                System.SetVesselOrbitColor(vessel);
            }
        }
    }
}
