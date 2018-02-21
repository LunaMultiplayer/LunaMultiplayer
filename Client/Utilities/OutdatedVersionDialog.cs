using LunaClient.Localization;
using UnityEngine;

namespace LunaClient.Utilities
{
    public class OutdatedVersionDialog
    {
        public static void SpawnDialog(string latestVersion, string currentVersion)
        {

            var disclaimerText = LocalizationContainer.OutdatedDialogText.RunningOutdated + "\n";
            disclaimerText += LocalizationContainer.OutdatedDialogText.Yours + $"{currentVersion}\n";
            disclaimerText += LocalizationContainer.OutdatedDialogText.Latest + $"{latestVersion}\n";

            PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog("OutdatedDialog", disclaimerText, LocalizationContainer.OutdatedDialogText.Title,
                    HighLogic.UISkin,
                    new Rect(.5f, .5f, 425f, 150f),
                    new DialogGUIFlexibleSpace(),
                    new DialogGUIVerticalLayout(
                        new DialogGUIHorizontalLayout(
                            new DialogGUIFlexibleSpace(),
                            new DialogGUIButton("Ok", delegate{}, true),
                            new DialogGUIFlexibleSpace()
                        )
                    )
                ),
                true,
                HighLogic.UISkin
            );
        }
    }
}
