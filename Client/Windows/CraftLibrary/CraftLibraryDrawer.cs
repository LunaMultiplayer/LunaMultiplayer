using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.SettingsSys;
using System;
using System.Linq;
using UnityEngine;

namespace LunaClient.Windows.CraftLibrary
{
    public partial class CraftLibraryWindow
    {
        #region Folders

        public override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            DrawRefreshAndUploadButton(() => System.MessageSender.RequestFolders(), ()=> DrawUploadScreen = true);
            GUILayout.Space(15);

            FoldersScrollPos = GUILayout.BeginScrollView(FoldersScrollPos, ScrollStyle);
            foreach (var folderName in Folders.ToArray())
            {
                DrawFolderButton(folderName);
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void DrawFolderButton(string folderName)
        {
            if (GUILayout.Toggle(SelectedFolder == folderName, folderName, ButtonStyle))
            {
                if (SelectedFolder != folderName)
                {
                    SelectedFolder = folderName;
                    System.MessageSender.RequestCraftList(SelectedFolder);
                }
            }
            else
            {
                if (SelectedFolder == folderName) SelectedFolder = null;
            }
        }

        #endregion

        #region Craft list

        public void DrawLibraryContent(int windowId)
        {
            //Always draw close button first
            DrawCloseButton(() => SelectedFolder = null, LibraryWindowRect);

            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            DrawRefreshButton(() => System.MessageSender.RequestCraftList(SelectedFolder));
            GUILayout.Space(15);

            if (string.IsNullOrEmpty(SelectedFolder)) return;

            LibraryScrollPos = GUILayout.BeginScrollView(LibraryScrollPos, ScrollStyle);
            if (SphCrafts.Any())
            {
                GUILayout.Label("SPH", BoldLabelStyle);
                for (var i = 0; i < SphCrafts.Count; i += 4)
                {
                    DrawCraftEntry(SphCrafts[i]);
                }
                GUILayout.Space(5);
            }
            if (VabCrafts.Any())
            {
                GUILayout.Label("VAB", BoldLabelStyle);
                for (var i = 0; i < VabCrafts.Count; i += 4)
                {
                    DrawCraftEntry(VabCrafts[i]);
                }
                GUILayout.Space(5);
            }
            if (SubAssemblyCrafts.Any())
            {
                GUILayout.Label("Subassembly", BoldLabelStyle);
                for (var i = 0; i < SubAssemblyCrafts.Count; i += 4)
                {
                    DrawCraftEntry(SubAssemblyCrafts[i]);
                }
                GUILayout.Space(5);
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void DrawCraftEntry(CraftBasicEntry craftBasicEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(craftBasicEntry.CraftName);

            if (craftBasicEntry.FolderName == SettingsSystem.CurrentSettings.PlayerName)
            {
                if (GUILayout.Button(DeleteIcon, ButtonStyle))
                {
                    System.MessageSender.DeleteCraft(craftBasicEntry);
                }
            }
            else
            {
                if (GUILayout.Button(SaveIcon, ButtonStyle))
                {
                    if (System.CraftDownloaded.TryGetValue(SelectedFolder, out var downloadedCraft) && !downloadedCraft.ContainsKey(craftBasicEntry.CraftName))
                        System.RequestCraft(craftBasicEntry);
                }
            }

            GUILayout.EndHorizontal();
        }

        #endregion

        #region Upload screen

        public void DrawUploadScreenContent(int windowId)
        {
            //Always draw close button first
            DrawCloseButton(() => DrawUploadScreen = false, UploadWindowRect);

            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            DrawRefreshButton(() => System.RefreshOwnCrafts());
            GUILayout.Space(15);
            
            UploadScrollPos = GUILayout.BeginScrollView(UploadScrollPos, ScrollStyle);
            for (var i = 0; i < System.OwnCrafts.Count; i += 4)
            {
                DrawUploadCraftEntry(System.OwnCrafts[i]);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawUploadCraftEntry(CraftEntry craftEntry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(craftEntry.CraftName);
            if (GUILayout.Button(UploadIcon, ButtonStyle))
            {
                System.SendCraft(craftEntry);
                DrawUploadScreen = false;
            }
            GUILayout.EndHorizontal();
        }

        #endregion

        private void DrawRefreshAndUploadButton(Action refreshAction, Action uploadAction)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(RefreshIcon, ButtonStyle)) refreshAction.Invoke();
            if (GUILayout.Button(UploadIcon, ButtonStyle)) uploadAction.Invoke();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
