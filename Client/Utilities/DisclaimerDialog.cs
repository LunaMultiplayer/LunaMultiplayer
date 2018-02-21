using LunaClient.Localization;
using LunaClient.Systems;
using LunaClient.Systems.SettingsSys;
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
                                    SystemsContainer.Get<MainSystem>().Enabled = true;
                                    SettingsSystem.SaveSettings();
                                }
                            ),
                            new DialogGUIFlexibleSpace(),
                            new DialogGUIButton(LocalizationContainer.DisclaimerDialogText.OpenUrl,
                                delegate
                                {
                                    Application.OpenURL("http://forum.kerbalspaceprogram.com/index.php?/topic/154851-add-on-posting-rules-march-8-2017/");
                                }
                                , false),
                            new DialogGUIFlexibleSpace(),
                            new DialogGUIButton(LocalizationContainer.DisclaimerDialogText.Decline,
                                delegate
                                {
                                    LunaLog.LogError("[LMP]: User did not accept disclaimer");
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
