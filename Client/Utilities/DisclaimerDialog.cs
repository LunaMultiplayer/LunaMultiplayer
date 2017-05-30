using LunaClient.Systems;
using LunaClient.Systems.SettingsSys;
using UnityEngine;

namespace LunaClient.Utilities
{
    public class DisclaimerDialog
    {
        public static void SpawnDialog()
        {
            var disclaimerText = "Luna Multi Player (LMP) shares the following personally identifiable information with the master server and any server you connect to.\n";
            disclaimerText += "a) Your player name you connect with.\n";
            disclaimerText += "b) Your player token (A randomly generated string to authenticate you).\n";
            disclaimerText += "c) Your IP address is logged on the server console.\n";
            disclaimerText += "\n";
            disclaimerText += "LMP does not contact any other computer than the server you are connecting to and the master server.\n";
            disclaimerText += "In order to use LMP, you must allow it to use this info\n";
            disclaimerText += "\n";
            disclaimerText += "For more information - see the KSP addon rules\n";

            PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog("DisclaimerWindow", disclaimerText, "LunaMultiPlayer - Disclaimer",
                    HighLogic.UISkin,
                    new Rect(.5f, .5f, 425f, 150f),
                    new DialogGUIFlexibleSpace(),
                    new DialogGUIVerticalLayout(
                        new DialogGUIHorizontalLayout(
                            new DialogGUIButton("Accept",
                                delegate
                                {
                                    SettingsSystem.CurrentSettings.DisclaimerAccepted = true;
                                    SystemsContainer.Get<MainSystem>().Enabled = true;
                                    SystemsContainer.Get<SettingsSystem>().SaveSettings();
                                }
                            ),
                            new DialogGUIFlexibleSpace(),
                            new DialogGUIButton("Open the KSP Addon rules in browser",
                                delegate
                                {
                                    Application.OpenURL("http://forum.kerbalspaceprogram.com/index.php?/topic/154851-add-on-posting-rules-march-8-2017/");
                                }
                                , false),
                            new DialogGUIFlexibleSpace(),
                            new DialogGUIButton("Decline",
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
