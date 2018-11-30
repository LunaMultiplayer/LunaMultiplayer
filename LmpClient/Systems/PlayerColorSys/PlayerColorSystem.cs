using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using System.Collections.Generic;
using UnityEngine;

namespace LmpClient.Systems.PlayerColorSys
{
    /// <summary>
    /// System that handles the player color
    /// </summary>
    public class PlayerColorSystem : MessageSystem<PlayerColorSystem, PlayerColorMessageSender, PlayerColorMessageHandler>
    {
        #region Fields

        public Color DefaultColor { get; } = XKCDColors.KSPNeutralUIGrey;
        public Dictionary<string, Color> PlayerColors { get; } = new Dictionary<string, Color>();
        public PlayerColorEvents PlayerColorEvents { get; } = new PlayerColorEvents();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(PlayerColorSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            MessageSender.SendPlayerColorToServer();

            GameEvents.onVesselCreate.Add(PlayerColorEvents.OnVesselCreated);
            VesselInitializeEvent.onVesselInitialized.Add(PlayerColorEvents.VesselInitialized);
            GameEvents.OnMapEntered.Add(PlayerColorEvents.MapEntered);
            LockEvent.onLockAcquire.Add(PlayerColorEvents.OnLockAcquire);
            LockEvent.onLockRelease.Add(PlayerColorEvents.OnLockRelease);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselCreate.Remove(PlayerColorEvents.OnVesselCreated);
            VesselInitializeEvent.onVesselInitialized.Remove(PlayerColorEvents.VesselInitialized);
            GameEvents.OnMapEntered.Remove(PlayerColorEvents.MapEntered);
            LockEvent.onLockAcquire.Remove(PlayerColorEvents.OnLockAcquire);
            LockEvent.onLockRelease.Remove(PlayerColorEvents.OnLockRelease);
            PlayerColors.Clear();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// When we create a vessel set it's orbit color to the player color
        /// </summary>
        public void SetVesselOrbitColor(Vessel vessel)
        {
            if (vessel == null) return;

            var vesselOwner = LockSystem.LockQuery.GetControlLockOwner(vessel.id);
            SetOrbitColor(vessel, vesselOwner == null ? DefaultColor : GetPlayerColor(vesselOwner));
        }

        public static Color GenerateRandomColor()
        {
            //Generates one of n distinct, luminous colours. They are spread across the spectrum using the golden ratio.
           const int n = 10;
            return Color.HSVToRGB((UnityEngine.Random.Range(0, n) * 0.618033988749895f) % 1.0f, 0.5f, 1.0f);
        }

        public Color GetPlayerColor(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
                return DefaultColor;

            if (playerName == SettingsSystem.CurrentSettings.PlayerName)
                return SettingsSystem.CurrentSettings.PlayerColor;

            return PlayerColors.ContainsKey(playerName) ? PlayerColors[playerName] : DefaultColor;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Sets the orbit color in a vessel
        /// </summary>
        private static void SetOrbitColor(Vessel vessel, Color colour)
        {
            if (vessel != null && vessel.orbitDriver != null)
            {
                vessel.orbitDriver.orbitColor = colour;
                if (vessel.orbitDriver.Renderer) vessel.orbitDriver.Renderer.SetColor(colour);
            }
        }
        
        #endregion
    }
}
