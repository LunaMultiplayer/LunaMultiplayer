using LunaClient.Systems.CraftLibrary;
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
            if (Crafts.Any())
            {
                for (var i = 0; i < Crafts.Count; i += 4)
                {
                    DrawCraftEntry(Crafts[i]);
                }
            }
            else
            {
                DrawWaitIcon(true);
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void DrawCraftEntry(CraftBasicEntry craftBasicEntry)
        {
            if (GUILayout.Button(craftBasicEntry.CraftName, ButtonStyle))
            {
                if (System.CraftDownloaded.TryGetValue(SelectedFolder, out var downloadedCraft) && !downloadedCraft.ContainsKey(craftBasicEntry.CraftName))
                    System.MessageSender.RequestCraft(SelectedFolder, craftBasicEntry.CraftName, craftBasicEntry.CraftType);
            }
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

            if (string.IsNullOrEmpty(SelectedFolder)) return;

            UploadScrollPos = GUILayout.BeginScrollView(UploadScrollPos, ScrollStyle);
            for (var i = 0; i < OwnCrafts.Count; i += 4)
            {
                DrawUploadCraftEntry(OwnCrafts[i]);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawUploadCraftEntry(CraftEntry craftEntry)
        {
            if (GUILayout.Button(craftEntry.CraftName, ButtonStyle))
            {
                System.MessageSender.SendCraft(craftEntry.FolderName, craftEntry.CraftName, craftEntry.CraftType, craftEntry.CraftData);
            }
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
