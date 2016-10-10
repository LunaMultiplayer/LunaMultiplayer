using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using KSP.UI.Screens;
using UnityEngine;

namespace LunaClient.Systems.Toolbar
{
    public class ToolbarSystem : System<ToolbarSystem>
    {
        public void ToolbarChanged()
        {
            if (Enabled)
            {
                DisableToolbar();
                EnableToolbar();
            }
        }

        #region Fields
        
        private bool StockDelayRegister { get; set; }
        private bool BlizzyRegistered { get; set; }
        private bool StockRegistered { get; set; }
        private Texture2D ButtonTexture { get; set; }
        private ApplicationLauncherButton StockLmpButton { get; set; }
        private IButton BlizzyButton { get; set; }

        private bool _enabled;
        public override bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (!_enabled && value)
                    EnableToolbar();
                else if (_enabled && !value)
                    DisableToolbar();

                _enabled = value;
            }
        }

        #endregion
        
        #region Private methods

        private void EnableToolbar()
        {
            ButtonTexture = GameDatabase.Instance.GetTexture("LunaMultiPlayer/Button/LMPButton", false);
            if (SettingsSystem.CurrentSettings.ToolbarType == LmpToolbarType.Disabled)
            {
                //Nothing!
            }
            if (SettingsSystem.CurrentSettings.ToolbarType == LmpToolbarType.ForceStock)
                EnableStockToolbar();
            if (SettingsSystem.CurrentSettings.ToolbarType == LmpToolbarType.BlizzyIfInstalled)
                if (ToolbarManager.ToolbarAvailable)
                    EnableBlizzyToolbar();
                else
                    EnableStockToolbar();
            if (SettingsSystem.CurrentSettings.ToolbarType == LmpToolbarType.BothIfInstalled)
            {
                if (ToolbarManager.ToolbarAvailable)
                    EnableBlizzyToolbar();
                EnableStockToolbar();
            }
        }

        private void DisableToolbar()
        {
            if (BlizzyRegistered)
                DisableBlizzyToolbar();
            if (StockRegistered)
                DisableStockToolbar();
        }

        private void EnableBlizzyToolbar()
        {
            BlizzyRegistered = true;
            BlizzyButton = ToolbarManager.Instance.add("LunaMultiPlayer", "GUIButton");
            BlizzyButton.OnClick += OnBlizzyClick;
            BlizzyButton.ToolTip = "Toggle LMP windows";
            BlizzyButton.TexturePath = "LunaMultiPlayer/Button/LMPButtonLow";
            BlizzyButton.Visibility = new GameScenesVisibility(GameScenes.EDITOR, GameScenes.FLIGHT,
                GameScenes.SPACECENTER, GameScenes.TRACKSTATION);
            Debug.Log("Registered blizzy toolbar");
        }

        private void DisableBlizzyToolbar()
        {
            BlizzyRegistered = false;
            BlizzyButton?.Destroy();
            Debug.Log("Unregistered blizzy toolbar");
        }

        private void EnableStockToolbar()
        {
            StockRegistered = true;
            if (ApplicationLauncher.Ready)
            {
                EnableStockForRealsies();
            }
            else
            {
                StockDelayRegister = true;
                GameEvents.onGUIApplicationLauncherReady.Add(EnableStockForRealsies);
            }
            Debug.Log("Registered stock toolbar");
        }

        private void EnableStockForRealsies()
        {
            if (StockDelayRegister)
            {
                StockDelayRegister = false;
                GameEvents.onGUIApplicationLauncherReady.Remove(EnableStockForRealsies);
            }
            StockLmpButton = ApplicationLauncher.Instance.AddModApplication(HandleButtonClick, HandleButtonClick,
                DoNothing, DoNothing, DoNothing, DoNothing, ApplicationLauncher.AppScenes.ALWAYS, ButtonTexture);
        }

        private void DisableStockToolbar()
        {
            StockRegistered = false;
            if (StockDelayRegister)
            {
                StockDelayRegister = false;
                GameEvents.onGUIApplicationLauncherReady.Remove(EnableStockForRealsies);
            }
            if (StockLmpButton != null)
                ApplicationLauncher.Instance.RemoveModApplication(StockLmpButton);
            Debug.Log("Unregistered stock toolbar");
        }

        private static void OnBlizzyClick(ClickEvent clickArgs) => HandleButtonClick();

        private static void HandleButtonClick()
            => MainSystem.Singleton.ToolbarShowGui = !MainSystem.Singleton.ToolbarShowGui;

        private static void DoNothing()
        {
        }

        #endregion
    }
}