using LmpClient.Localization;
using LmpGlobal;
using System;
using UnityEngine;

namespace LmpClient.Utilities
{
    internal class CompatibilityChecker
    {
        private static readonly Version KspVersion = new Version(Versioning.version_major, Versioning.version_minor, Versioning.Revision);

        public static bool IsCompatible()
        {
            return KspVersion >= KspCompatible.MinKspVersion && KspVersion <= KspCompatible.MaxKspVersion;
        }

        public static void SpawnDialog()
        {
            if (IsCompatible()) return;

            PopupDialog.SpawnPopupDialog(
                new MultiOptionDialog("CompatibilityWindow", string.Empty, LocalizationContainer.CompatibleDialogText.Title,
                    HighLogic.UISkin,
                    new Rect(.5f, .5f, 425f, 150f),
                    new DialogGUIVerticalLayout(
                        new DialogGUIFlexibleSpace(),
                        new DialogGUILabel(LocalizationContainer.CompatibleDialogText.Text),
                        new DialogGUIFlexibleSpace(),
                        new DialogGUIHorizontalLayout(
                            new DialogGUIFlexibleSpace(),
                            new DialogGUIButton(LocalizationContainer.CompatibleDialogText.Accept, null),
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
