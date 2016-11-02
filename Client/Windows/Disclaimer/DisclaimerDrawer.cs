using LunaClient.Systems.SettingsSys;
using UnityEngine;

namespace LunaClient.Windows.Disclaimer
{
    public partial class DisclaimerWindow
    {
        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            var disclaimerText = "Luna Multi Player (LMP) shares the following personally identifiable information with the master server and any server you connect to.\n";
            disclaimerText += "a) Your player name you connect with.\n";
            disclaimerText += "b) Your player token (A randomly generated string to authenticate you).\n";
            disclaimerText += "c) Your IP address is logged on the server console.\n";
            disclaimerText += "\n";
            disclaimerText += "LMP does not contact any other computer than the server you are connecting to and the master server.\n";
            disclaimerText += "In order to use LMP, you must allow it to use this info\n";
            disclaimerText += "\n";
            disclaimerText += "For more information - see the KSP addon rules\n";
            GUILayout.Label(disclaimerText);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open the KSP Addon rules in the browser"))
                Application.OpenURL(
                    "http://forum.kerbalspaceprogram.com/threads/87841-Add-on-Posting-Rules-July-24th-2014-going-into-effect-August-21st-2014!");
            if (GUILayout.Button("I accept - Enable LMP"))
            {
                Display = false;
                SettingsSystem.CurrentSettings.DisclaimerAccepted = true;
                MainSystem.Singleton.Enabled = true;
                SettingsSystem.Singleton.SaveSettings();
            }
            if (GUILayout.Button("I decline - Disable LMP"))
            {
                MainSystem.Singleton.ShowGui = false;
                Display = false;
            }
            GUILayout.EndVertical();
        }
    }
}