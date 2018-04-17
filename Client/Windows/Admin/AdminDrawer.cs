using LunaClient.Localization;
using LunaClient.Systems.Admin;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using UnityEngine;
using System.Threading;

namespace LunaClient.Windows.Admin
{
    public partial class AdminWindow
    {
        public override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.AdminWindowText.Password, LabelStyle);
            AdminSystem.Singleton.AdminPassword = GUILayout.PasswordField(AdminSystem.Singleton.AdminPassword, '*', 30, TextAreaStyle, GUILayout.Width(200)); // Max 32 characters
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUI.enabled = !string.IsNullOrEmpty(AdminSystem.Singleton.AdminPassword);

            ScrollPos = GUILayout.BeginScrollView(ScrollPos, ScrollStyle);
            foreach (var player in StatusSystem.Singleton.PlayerStatusList.Keys)
            {
                if (player == SettingsSystem.CurrentSettings.PlayerName) continue;
                DrawPlayerLine(player);
            }
            GUILayout.EndScrollView();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(DekesslerBigIcon, ButtonStyle))
            {
                AdminSystem.Singleton.MessageSender.SendDekesslerMsg();
            }
            if (GUILayout.Button(NukeBigIcon, ButtonStyle))
            {
                AdminSystem.Singleton.MessageSender.SendNukeMsg();
            }
            if (GUILayout.Button(RestartServerIcon, ButtonStyle))
            {
                //Need to clear tooltip text because otherwise it would be still displayed in main menu after disconnect
                //RestartServerIcon.tooltip = "";
                //Close admin window so it doesn't show up after reconnect
                //Singleton.OnCloseButton();
                AdminSystem.Singleton.MessageSender.SendServerRestartMsg();
            }

            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawPlayerLine(string playerName)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(playerName, LabelStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(BanIcon, ButtonStyle))
            {
                _selectedPlayer = playerName;
                _banMode = true;
            }
            if (GUILayout.Button(KickIcon, ButtonStyle))
            {
                _selectedPlayer = playerName;
                _banMode = false;
            }
            GUILayout.EndHorizontal();
        }

        #region Confirmation Dialog

        public void DrawConfirmationDialog(int windowId)
        {
            //Always draw close button first
            DrawCloseButton(() => { _selectedPlayer = null; _reason = string.Empty; }, _confirmationWindowRect);

            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.Label(_banMode ? LocalizationContainer.AdminWindowText.BanText : LocalizationContainer.AdminWindowText.KickText, LabelOptions);
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.AdminWindowText.Reason, LabelOptions);
            _reason = GUILayout.TextField(_reason, 255, TextAreaStyle, GUILayout.Width(255));
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (_banMode)
            {
                if (GUILayout.Button(BanBigIcon, ButtonStyle, GUILayout.Width(255)))
                {
                    AdminSystem.Singleton.MessageSender.SendBanPlayerMsg(_selectedPlayer, _reason);
                    _selectedPlayer = null;
                    _reason = string.Empty;
                }
            }
            else
            {
                if (GUILayout.Button(KickBigIcon, ButtonStyle, GUILayout.Width(255)))
                {
                    AdminSystem.Singleton.MessageSender.SendKickPlayerMsg(_selectedPlayer, _reason);
                    _selectedPlayer = null;
                    _reason = string.Empty;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        #endregion
    }
}
