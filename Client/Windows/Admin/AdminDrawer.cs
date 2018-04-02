using LunaClient.Localization;
using LunaClient.Systems.Admin;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using System;
using UnityEngine;

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
            AdminSystem.Singleton.AdminPassword = GUILayout.PasswordField(AdminSystem.Singleton.AdminPassword, '*', 30, TextAreaStyle); // Max 32 characters
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

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
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawPlayerLine(string playerName)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(playerName, LabelStyle);
            if (GUILayout.Button(BanIcon, ButtonStyle))
            {
                SpawnDialog(LocalizationContainer.AdminWindowText.BanConfirmDialogTitle, LocalizationContainer.AdminWindowText.BanText,
                    () => AdminSystem.Singleton.MessageSender.SendBanPlayerMsg(playerName));
            }
            if (GUILayout.Button(KickIcon, ButtonStyle))
            {
                SpawnDialog(LocalizationContainer.AdminWindowText.KickConfirmDialogTitle, LocalizationContainer.AdminWindowText.KickText,
                    () => AdminSystem.Singleton.MessageSender.SendKickPlayerMsg(playerName));
            }
            GUILayout.EndHorizontal();
        }

        private void SpawnDialog(string title, string text, Action confirmAction)
        {
            PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog("ConfirmDialog", text, title,
                    HighLogic.UISkin,
                    new Rect(.5f, .5f, 425f, 150f),
                    new DialogGUIFlexibleSpace(),
                    new DialogGUIVerticalLayout(
                        new DialogGUIHorizontalLayout(
                            new DialogGUIButton("YES",
                                confirmAction.Invoke
                            ),
                            new DialogGUIFlexibleSpace(),
                            new DialogGUIButton("NO",
                                delegate
                                {
                                }
                            )
                        )
                    )
                ),
                true,
                HighLogic.UISkin
            );
        }
    }
}
