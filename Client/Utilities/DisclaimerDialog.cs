using LunaClient.Localization;
using LunaClient.Systems.SettingsSys;
using LunaClient.Update;
using UnityEngine;

namespace LunaClient.Utilities
{
    public class DisclaimerDialog
    {
        public static void SpawnDialog()
        {
            PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog("DisclaimerWindow", LocalizationContainer.DisclaimerDialogText.Text, LocalizationContainer.DisclaimerDialogText.Title,
                    HighLogic.UISkin, 
                    new Rect(.5f, .5f, 425f, 150f),
                    new DialogGUIFlexibleSpace(),
                    new DialogGUIVerticalLayout(
                        new DialogGUIHorizontalLayout(
                            new DialogGUIButton(LocalizationContainer.DisclaimerDialogText.Accept,
                                delegate
                                {
                                    SettingsSystem.CurrentSettings.DisclaimerAccepted = true;
                                    MainSystem.Singleton.Enabled = true;
                                    SettingsSystem.SaveSettings();
                                    MainSystem.Singleton.StartCoroutine(UpdateHandler.CheckForUpdates());
                                }
                            ),
                            new DialogGUIFlexibleSpace(),
                            new DialogGUIButton(LocalizationContainer.DisclaimerDialogText.Decline,
                                delegate
                                {
                                    LunaLog.LogError("[LMP]: User did not accept disclaimer");
                                }
                            ),
                            new DialogGUIFlexibleSpace(),
                            new DialogGUIButton(LocalizationContainer.GetCurrentLanguageAsText(),
                                delegate
                                {
                                    LocalizationContainer.LoadLanguage(LocalizationContainer.GetNextLanguage());
                                    SettingsSystem.CurrentSettings.Language = LocalizationContainer.CurrentLanguage;
                                    SettingsSystem.SaveSettings();
                                    SpawnDialog();
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
