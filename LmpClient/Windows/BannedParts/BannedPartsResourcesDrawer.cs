using LmpClient.Localization;
using LmpClient.Systems.SettingsSys;
using System.Linq;
using UnityEngine;

namespace LmpClient.Windows.BannedParts
{
    public partial class BannedPartsResourcesWindow
    {
        protected override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            if (_partCount > 0)
            {
                GUILayout.Label($"{LocalizationContainer.BannedPartsResourcesWindowText.TooManyParts} {SettingsSystem.ServerSettings.MaxVesselParts}", BoldRedLabelStyle);
            }
            else
            {
                GUILayout.Label($"{_vesselName} {LocalizationContainer.BannedPartsResourcesWindowText.Text}", BoldRedLabelStyle);
                GUILayout.Space(5);

                GUILayout.BeginVertical();
                ScrollPos = GUILayout.BeginScrollView(ScrollPos);
                if (_bannedParts.Any())
                {
                    GUILayout.Label(LocalizationContainer.BannedPartsResourcesWindowText.BannedParts, BoldRedLabelStyle);
                    foreach (var bannedPart in _bannedParts)
                    {
                        GUILayout.Label(bannedPart);
                    }
                }
                if (_bannedResources.Any())
                {
                    GUILayout.Label(LocalizationContainer.BannedPartsResourcesWindowText.BannedResources, BoldRedLabelStyle);
                    foreach (var bannedResource in _bannedResources)
                    {
                        GUILayout.Label(bannedResource);
                    }
                }
                if (_partCount > SettingsSystem.ServerSettings.MaxVesselParts)
                {
                    GUILayout.Label($"{LocalizationContainer.BannedPartsResourcesWindowText.TooManyParts} {SettingsSystem.ServerSettings.MaxVesselParts}", BoldRedLabelStyle);
                }

                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();
        }
    }
}
