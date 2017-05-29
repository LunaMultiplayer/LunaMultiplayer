using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Random = System.Random;

namespace LunaClient.Systems.PlayerColorSys
{
    /// <summary>
    /// System that handles the player color
    /// </summary>
    public class PlayerColorSystem : MessageSystem<PlayerColorSystem, PlayerColorMessageSender, PlayerColorMessageHandler>
    {
        #region Fields

        public Color DefaultColor { get; } = Color.grey;
        public Dictionary<string, Color> PlayerColors { get; } = new Dictionary<string, Color>();
        public PlayerColorEvents PlayerColorEvents { get; } = new PlayerColorEvents();

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselCreate.Add(PlayerColorEvents.SetVesselOrbitColor);
            SystemsContainer.Get<LockSystem>().RegisterAcquireHook(PlayerColorEvents.OnLockAcquire);
            SystemsContainer.Get<LockSystem>().RegisterReleaseHook(PlayerColorEvents.OnLockRelease);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselCreate.Remove(PlayerColorEvents.SetVesselOrbitColor);
            SystemsContainer.Get<LockSystem>().UnregisterAcquireHook(PlayerColorEvents.OnLockAcquire);
            SystemsContainer.Get<LockSystem>().UnregisterReleaseHook(PlayerColorEvents.OnLockRelease);
            PlayerColors.Clear();

        }

        #endregion

        #region Public methods

        public static Color GenerateRandomColor()
        {
            switch (new Random().Next() % 17)
            {
                case 0:
                    return Color.red;
                case 1:
                    return new Color(1, 0, 0.5f, 1); //Rosy pink
                case 2:
                    return new Color(0.6f, 0, 0.5f, 1); //OU Crimson
                case 3:
                    return new Color(1, 0.5f, 0, 1); //Orange
                case 4:
                    return Color.yellow;
                case 5:
                    return new Color(1, 0.84f, 0, 1); //Gold
                case 6:
                    return Color.green;
                case 7:
                    return new Color(0, 0.651f, 0.576f, 1); //Persian Green
                case 8:
                    return new Color(0, 0.651f, 0.576f, 1); //Persian Green
                case 9:
                    return new Color(0, 0.659f, 0.420f, 1); //Jade
                case 10:
                    return new Color(0.043f, 0.855f, 0.318f, 1); //Malachite
                case 11:
                    return Color.cyan;
                case 12:
                    return new Color(0.537f, 0.812f, 0.883f, 1); //Baby blue;
                case 13:
                    return new Color(0, 0.529f, 0.741f, 1); //NCS blue
                case 14:
                    return new Color(0.255f, 0.412f, 0.882f, 1); //Royal Blue
                case 15:
                    return new Color(0.5f, 0, 1, 1); //Violet
                default:
                    return Color.magenta;
            }
        }

        public Color GetPlayerColor(string playerName)
        {
            if (playerName == SettingsSystem.CurrentSettings.PlayerName)
                return SettingsSystem.CurrentSettings.PlayerColor;

            return PlayerColors.ContainsKey(playerName) ? PlayerColors[playerName] : DefaultColor;
        }

        #endregion

        #region Private methods

        public string ConvertColorToString(Color convertColour)
        {
            var returnArray = new[]
            {
                convertColour.r.ToString(CultureInfo.InvariantCulture),
                convertColour.g.ToString(CultureInfo.InvariantCulture),
                convertColour.b.ToString(CultureInfo.InvariantCulture)
            };

            return string.Join(";", returnArray);
        }

        public Color ConvertStringToColor(string convertArray)
        {
            var array = convertArray.Split(';');

            return new Color(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]));
        }

        #endregion
    }
}