using UnityEngine;

namespace LunaClient.Utilities
{
    public class OutdatedVersionDialog
    {
        public static void SpawnDialog(string latestVersion, string currentVersion)
        {
            var disclaimerText = "You are running an outdated version of LMP.\n";
            disclaimerText += $"Yours {currentVersion}\n";
            disclaimerText += $"Latest {latestVersion}\n";

            PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog("OutdatedDialog", disclaimerText, "LunaMultiPlayer - Update available",
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
