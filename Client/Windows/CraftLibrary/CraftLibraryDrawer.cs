using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.SettingsSys;
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
            DrawRefreshButton(() => System.MessageSender.RequestFolders());
            DrawFolderButton(SettingsSystem.CurrentSettings.PlayerName);
            GUILayout.Space(10);

            FoldersScrollPos = GUILayout.BeginScrollView(FoldersScrollPos, ScrollStyle);
            foreach (var folderName in System.CraftInfo.Keys.ToArray())
            {
                if (folderName != SettingsSystem.CurrentSettings.PlayerName)
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
            if (System.CraftInfo.TryGetValue(SelectedFolder, out var miniatures))
            {
                var craftList = miniatures.Values.OrderBy(m => m.CraftType).ToArray();
                if (!craftList.Any())
                {
                    DrawWaitIcon();
                }
                for (var i = 0; i < craftList.Length; i += 4)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.FlexibleSpace();
                    DrawCraftEntry(craftList[i]);
                    GUILayout.FlexibleSpace();

                    if (craftList.Length > i + 1)
                    {
                        GUILayout.FlexibleSpace();
                        DrawCraftEntry(craftList[i + 1]);
                        GUILayout.FlexibleSpace();
                    }

                    if (craftList.Length > i + 2)
                    {
                        GUILayout.FlexibleSpace();
                        DrawCraftEntry(craftList[i + 2]);
                        GUILayout.FlexibleSpace();
                    }

                    if (craftList.Length > i + 3)
                    {
                        GUILayout.FlexibleSpace();
                        DrawCraftEntry(craftList[i + 3]);
                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                DrawWaitIcon();
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
    }
}
