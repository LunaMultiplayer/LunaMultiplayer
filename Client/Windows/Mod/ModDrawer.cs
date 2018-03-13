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

            if (ModSystem.Singleton.MandatoryFilesNotFound.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.MandatoryModsNotFound, LabelStyle);
                ScrollPos = GUILayout.BeginScrollView(ScrollPos, ScrollStyle);
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
                GUILayout.Space(10);
            }

            if (ModSystem.Singleton.MandatoryFilesDifferentSha.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.MandatoryModsDifferentShaFound, LabelStyle);
                ScrollPos = GUILayout.BeginScrollView(ScrollPos, ScrollStyle);
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
                GUILayout.Space(10);
            }

            if (ModSystem.Singleton.ForbiddenFilesFound.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.ForbiddenFilesFound, LabelStyle);
                ScrollPos = GUILayout.BeginScrollView(ScrollPos, ScrollStyle);
                foreach (var mod in ModSystem.Singleton.ForbiddenFilesFound)
                {
                    GUILayout.Label(mod.FilePath, LabelStyle);
                    if (!string.IsNullOrEmpty(mod.Text))
                        GUILayout.Label(mod.Text, LabelStyle);
                }

                GUILayout.EndScrollView();
                GUILayout.Space(10);
            }

            if (ModSystem.Singleton.NonListedFilesFound.Any())
            {
                GUILayout.Label(LocalizationContainer.ModWindowText.NonListedFilesFound, LabelStyle);
                ScrollPos = GUILayout.BeginScrollView(ScrollPos, ScrollStyle);
                foreach (var mod in ModSystem.Singleton.NonListedFilesFound)
                {
                    GUILayout.Label(mod, LabelStyle);
                }

                GUILayout.EndScrollView();
                GUILayout.Space(10);
            }

            if (GUILayout.Button(LocalizationContainer.ModWindowText.Close, ButtonStyle))
                Display = false;
            GUILayout.EndVertical();
        }
    }
}