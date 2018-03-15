using LunaClient.Localization;
using LunaClient.Systems.Mod;
using UniLinq;
using UnityEngine;

namespace LunaClient.Windows.Mod
{
    public partial class ModWindow
    {
        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            GUILayout.Space(10);

            ScrollPos = GUILayout.BeginScrollView(_missingExpansionsScrollPos, ScrollStyle);

            if (ModSystem.Singleton.MissingExpansions.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.MissingExpansions, BoldLabelStyle);
                GUILayout.BeginHorizontal(BoxStyle);
                _missingExpansionsScrollPos = GUILayout.BeginScrollView(_missingExpansionsScrollPos, ScrollStyle);
                foreach (var expansion in ModSystem.Singleton.MissingExpansions)
                {
                    GUILayout.Label(expansion, LabelStyle);
                }

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            if (ModSystem.Singleton.MandatoryFilesNotFound.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.MandatoryModsNotFound, BoldLabelStyle);
                GUILayout.BeginHorizontal(BoxStyle);
                _mandatoryFilesNotFoundScrollPos = GUILayout.BeginScrollView(_mandatoryFilesNotFoundScrollPos, ScrollStyle);
                foreach (var mod in ModSystem.Singleton.MandatoryFilesNotFound)
                {
                    GUILayout.Label(mod.FilePath, LabelStyle);
                    if (!string.IsNullOrEmpty(mod.Text))
                        GUILayout.Label(mod.Text, LabelStyle);
                    if (!string.IsNullOrEmpty(mod.Link))
                    {
                        if (GUILayout.Button(LocalizationContainer.ModWindowText.Link))
                            Application.OpenURL(mod.Link);
                    }
                }

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            if (ModSystem.Singleton.MandatoryFilesDifferentSha.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.MandatoryModsDifferentShaFound, BoldLabelStyle);
                GUILayout.BeginHorizontal(BoxStyle);
                _mandatoryFilesDifferentShaScrollPos = GUILayout.BeginScrollView(_mandatoryFilesDifferentShaScrollPos, ScrollStyle);
                foreach (var mod in ModSystem.Singleton.MandatoryFilesDifferentSha)
                {
                    GUILayout.Label(mod.FilePath, LabelStyle);
                    GUILayout.Label(mod.Sha, LabelStyle);
                    if (!string.IsNullOrEmpty(mod.Text))
                        GUILayout.Label(mod.Text, LabelStyle);
                    if (!string.IsNullOrEmpty(mod.Link))
                    {
                        if (GUILayout.Button(LocalizationContainer.ModWindowText.Link))
                            Application.OpenURL(mod.Link);
                    }
                }

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            if (ModSystem.Singleton.ForbiddenFilesFound.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.ForbiddenFilesFound, BoldLabelStyle);
                GUILayout.BeginHorizontal(BoxStyle);
                _forbiddenFilesScrollPos = GUILayout.BeginScrollView(_forbiddenFilesScrollPos, ScrollStyle);
                foreach (var mod in ModSystem.Singleton.ForbiddenFilesFound)
                {
                    GUILayout.Label(mod.FilePath, LabelStyle);
                    if (!string.IsNullOrEmpty(mod.Text))
                        GUILayout.Label(mod.Text, LabelStyle);
                }

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            if (ModSystem.Singleton.NonListedFilesFound.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.NonListedFilesFound, BoldLabelStyle);
                GUILayout.BeginHorizontal(BoxStyle);
                _nonListedFilesScrollPos = GUILayout.BeginScrollView(_nonListedFilesScrollPos, ScrollStyle);
                foreach (var mod in ModSystem.Singleton.NonListedFilesFound)
                {
                    GUILayout.Label(mod, LabelStyle);
                }

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            if (ModSystem.Singleton.MandatoryPartsNotFound.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.MandatoryPartsNotFound, BoldLabelStyle);
                GUILayout.BeginHorizontal(BoxStyle);
                _mandatoryPartsScrollPos = GUILayout.BeginScrollView(_mandatoryPartsScrollPos, ScrollStyle);
                foreach (var part in ModSystem.Singleton.MandatoryPartsNotFound)
                {
                    GUILayout.Label(part.PartName, LabelStyle);
                    if (!string.IsNullOrEmpty(part.Text))
                        GUILayout.Label(part.Text, LabelStyle);
                    if (!string.IsNullOrEmpty(part.Link))
                    {
                        if (GUILayout.Button(LocalizationContainer.ModWindowText.Link))
                            Application.OpenURL(part.Link);
                    }
                }

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            if (ModSystem.Singleton.ForbiddenPartsFound.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.ForbiddenPartsFound, BoldLabelStyle);
                GUILayout.BeginHorizontal(BoxStyle);
                _forbiddenPartsScrollPos = GUILayout.BeginScrollView(_forbiddenPartsScrollPos, ScrollStyle);
                foreach (var part in ModSystem.Singleton.ForbiddenPartsFound)
                {
                    GUILayout.Label(part.PartName, LabelStyle);
                    if (!string.IsNullOrEmpty(part.Text))
                        GUILayout.Label(part.Text, LabelStyle);
                }

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button(LocalizationContainer.ModWindowText.Close, ButtonStyle))
                Display = false;
            GUILayout.EndVertical();
        }
    }
}