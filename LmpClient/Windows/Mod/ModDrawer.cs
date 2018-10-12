using LmpClient.Localization;
using LmpClient.Systems.Mod;
using UniLinq;
using UnityEngine;

namespace LmpClient.Windows.Mod
{
    public partial class ModWindow
    {
        protected override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            GUILayout.Space(10);

            ScrollPos = GUILayout.BeginScrollView(_missingExpansionsScrollPos);

            if (ModSystem.Singleton.MissingExpansions.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.MissingExpansions, BoldRedLabelStyle);
                GUILayout.BeginHorizontal();
                _missingExpansionsScrollPos = GUILayout.BeginScrollView(_missingExpansionsScrollPos);
                foreach (var expansion in ModSystem.Singleton.MissingExpansions)
                {
                    GUILayout.Label(expansion);
                }

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            if (ModSystem.Singleton.MandatoryFilesNotFound.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.MandatoryModsNotFound, BoldRedLabelStyle);
                GUILayout.BeginHorizontal();
                _mandatoryFilesNotFoundScrollPos = GUILayout.BeginScrollView(_mandatoryFilesNotFoundScrollPos);
                foreach (var mod in ModSystem.Singleton.MandatoryFilesNotFound)
                {
                    GUILayout.Label(mod.FilePath);
                    if (!string.IsNullOrEmpty(mod.Text))
                        GUILayout.Label(mod.Text);
                    if (!string.IsNullOrEmpty(mod.Link))
                    {
                        if (GUILayout.Button(LocalizationContainer.ModWindowText.Link, HyperlinkLabelStyle))
                            Application.OpenURL(mod.Link);
                    }
                }

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            if (ModSystem.Singleton.MandatoryFilesDifferentSha.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.MandatoryModsDifferentShaFound, BoldRedLabelStyle);
                GUILayout.BeginHorizontal();
                _mandatoryFilesDifferentShaScrollPos = GUILayout.BeginScrollView(_mandatoryFilesDifferentShaScrollPos);
                foreach (var mod in ModSystem.Singleton.MandatoryFilesDifferentSha)
                {
                    GUILayout.Label(mod.FilePath);
                    GUILayout.Label(mod.Sha);
                    if (!string.IsNullOrEmpty(mod.Text))
                        GUILayout.Label(mod.Text);
                    if (!string.IsNullOrEmpty(mod.Link))
                    {
                        if (GUILayout.Button(LocalizationContainer.ModWindowText.Link, HyperlinkLabelStyle))
                            Application.OpenURL(mod.Link);
                    }
                }

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            if (ModSystem.Singleton.ForbiddenFilesFound.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.ForbiddenFilesFound, BoldRedLabelStyle);
                GUILayout.BeginHorizontal();
                _forbiddenFilesScrollPos = GUILayout.BeginScrollView(_forbiddenFilesScrollPos);
                foreach (var mod in ModSystem.Singleton.ForbiddenFilesFound)
                {
                    GUILayout.Label(mod.FilePath);
                    if (!string.IsNullOrEmpty(mod.Text))
                        GUILayout.Label(mod.Text);
                }

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            if (ModSystem.Singleton.NonListedFilesFound.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.NonListedFilesFound, BoldRedLabelStyle);
                GUILayout.BeginHorizontal();
                _nonListedFilesScrollPos = GUILayout.BeginScrollView(_nonListedFilesScrollPos);
                foreach (var mod in ModSystem.Singleton.NonListedFilesFound)
                {
                    GUILayout.Label(mod);
                }

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            if (ModSystem.Singleton.MandatoryPartsNotFound.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.MandatoryPartsNotFound, BoldRedLabelStyle);
                GUILayout.BeginHorizontal();
                _mandatoryPartsScrollPos = GUILayout.BeginScrollView(_mandatoryPartsScrollPos);
                foreach (var part in ModSystem.Singleton.MandatoryPartsNotFound)
                {
                    GUILayout.Label(part.PartName);
                    if (!string.IsNullOrEmpty(part.Text))
                        GUILayout.Label(part.Text);
                    if (!string.IsNullOrEmpty(part.Link))
                    {
                        if (GUILayout.Button(LocalizationContainer.ModWindowText.Link, HyperlinkLabelStyle))
                            Application.OpenURL(part.Link);
                    }
                }

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            if (ModSystem.Singleton.ForbiddenPartsFound.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.ForbiddenPartsFound, BoldRedLabelStyle);
                GUILayout.BeginHorizontal();
                _forbiddenPartsScrollPos = GUILayout.BeginScrollView(_forbiddenPartsScrollPos);
                foreach (var part in ModSystem.Singleton.ForbiddenPartsFound)
                {
                    GUILayout.Label(part.PartName);
                    if (!string.IsNullOrEmpty(part.Text))
                        GUILayout.Label(part.Text);
                }

                GUILayout.EndScrollView();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}
