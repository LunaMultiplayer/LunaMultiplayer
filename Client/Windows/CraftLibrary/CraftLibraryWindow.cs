using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Systems.CraftLibrary;
using LunaCommon.Enums;
using LunaCommon.Time;
using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace LunaClient.Windows.CraftLibrary
{
    public partial class CraftLibraryWindow : SystemWindow<CraftLibraryWindow, CraftLibrarySystem>
    {
        #region Fields

        private const int UpdateIntervalMs = 1500;

        private const float FoldersWindowHeight = 300;
        private const float FoldersWindowWidth = 200;
        private const float LibraryWindowHeight = 300;
        private const float LibraryWindowWidth = 400;
        private const float UploadWindowHeight = 300;
        private const float UploadWindowWidth = 400;

        private static Rect _libraryWindowRect;
        private static Rect _uploadWindowRect;

        private static GUILayoutOption[] _foldersLayoutOptions;
        private static GUILayoutOption[] _libraryLayoutOptions;
        private static GUILayoutOption[] _uploadLayoutOptions;

        private static Vector2 _foldersScrollPos;
        private static Vector2 _libraryScrollPos;
        private static Vector2 _uploadScrollPos;

        private static string _selectedFolder;
        private static bool _drawUploadScreen;

        private static DateTime _lastGuiUpdateTime = DateTime.MinValue;

        private static readonly List<CraftBasicEntry> VabCrafts = new List<CraftBasicEntry>();
        private static readonly List<CraftBasicEntry> SphCrafts = new List<CraftBasicEntry>();
        private static readonly List<CraftBasicEntry> SubAssemblyCrafts = new List<CraftBasicEntry>();

        #endregion

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display && MainSystem.NetworkState >= ClientState.Running && HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set
            {
                if (!value) Reset();

                if (value && !_display && System.CraftInfo.Count == 0)
                    System.MessageSender.SendRequestFoldersMsg();
                base.Display = _display = value;
            }
        }

        public override void Update()
        {
            base.Update();
            if (!Display) return;

            if (TimeUtil.IsInInterval(ref _lastGuiUpdateTime, UpdateIntervalMs))
            {
                VabCrafts.Clear();
                SphCrafts.Clear();
                SubAssemblyCrafts.Clear();

                if (!string.IsNullOrEmpty(_selectedFolder) && System.CraftInfo.TryGetValue(_selectedFolder, out var craftsDictionary))
                {
                    var allValues = craftsDictionary.Values.GroupBy(v=> v.CraftType).ToArray();
                    foreach (var groupedValues in allValues)
                    {
                        switch (groupedValues.Key)
                        {
                            case CraftType.Vab:
                                VabCrafts.AddRange(groupedValues);
                                break;
                            case CraftType.Sph:
                                SphCrafts.AddRange(groupedValues);
                                break;
                            case CraftType.Subassembly:
                                SubAssemblyCrafts.AddRange(groupedValues);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
        }

        public override void OnGui()
        {
            base.OnGui();
            if (Display)
            {
                WindowRect = FixWindowPos(GUILayout.Window(6707 + MainSystem.WindowOffset,
                    WindowRect, DrawContent, LocalizationContainer.CraftLibraryWindowText.Folders, WindowStyle, _foldersLayoutOptions));
            }

            if (Display && !string.IsNullOrEmpty(_selectedFolder) && System.CraftInfo.ContainsKey(_selectedFolder))
            {
                _libraryWindowRect = FixWindowPos(GUILayout.Window(6708 + MainSystem.WindowOffset,
                    _libraryWindowRect, DrawLibraryContent, $"{_selectedFolder} {LocalizationContainer.CraftLibraryWindowText.Crafts}", WindowStyle,
                    _libraryLayoutOptions));
            }

            if (Display && _drawUploadScreen)
            {
                _uploadWindowRect = FixWindowPos(GUILayout.Window(6709 + MainSystem.WindowOffset,
                    _uploadWindowRect, DrawUploadScreenContent, LocalizationContainer.CraftLibraryWindowText.Upload, WindowStyle,
                    _uploadLayoutOptions));
            }

            CheckWindowLock();
        }
        
        public override void SetStyles()
        {
            WindowRect = new Rect(50, Screen.height / 2f - FoldersWindowHeight / 2f, FoldersWindowWidth, FoldersWindowHeight);
            _libraryWindowRect = new Rect(Screen.width / 2f - LibraryWindowWidth / 2f, Screen.height / 2f - LibraryWindowHeight / 2f, LibraryWindowWidth, LibraryWindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);

            _foldersLayoutOptions = new GUILayoutOption[4];
            _foldersLayoutOptions[0] = GUILayout.MinWidth(FoldersWindowWidth);
            _foldersLayoutOptions[1] = GUILayout.MaxWidth(FoldersWindowWidth);
            _foldersLayoutOptions[2] = GUILayout.MinHeight(FoldersWindowHeight);
            _foldersLayoutOptions[3] = GUILayout.MaxHeight(FoldersWindowHeight);

            _libraryLayoutOptions = new GUILayoutOption[4];
            _libraryLayoutOptions[0] = GUILayout.MinWidth(LibraryWindowWidth);
            _libraryLayoutOptions[1] = GUILayout.MaxWidth(LibraryWindowWidth);
            _libraryLayoutOptions[2] = GUILayout.MinHeight(LibraryWindowHeight);
            _libraryLayoutOptions[3] = GUILayout.MaxHeight(LibraryWindowHeight);

            _uploadLayoutOptions = new GUILayoutOption[4];
            _uploadLayoutOptions[0] = GUILayout.MinWidth(UploadWindowWidth);
            _uploadLayoutOptions[1] = GUILayout.MaxWidth(UploadWindowWidth);
            _uploadLayoutOptions[2] = GUILayout.MinHeight(UploadWindowHeight);
            _uploadLayoutOptions[3] = GUILayout.MaxHeight(UploadWindowHeight);
        }

        public override void RemoveWindowLock()
        {
            if (IsWindowLocked)
            {
                IsWindowLocked = false;
                InputLockManager.RemoveControlLock("LMP_CraftLibraryLock");
            }
        }

        public void CheckWindowLock()
        {
            if (Display)
            {
                if (MainSystem.NetworkState < ClientState.Running || HighLogic.LoadedSceneIsFlight)
                {
                    RemoveWindowLock();
                    return;
                }

                Vector2 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;

                var shouldLock = WindowRect.Contains(mousePos)
                                 || !string.IsNullOrEmpty(_selectedFolder) && _libraryWindowRect.Contains(mousePos)
                                 || _drawUploadScreen && _uploadWindowRect.Contains(mousePos);

                if (shouldLock && !IsWindowLocked)
                {
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_CraftLibraryLock");
                    IsWindowLocked = true;
                }
                if (!shouldLock && IsWindowLocked)
                    RemoveWindowLock();
            }

            if (!Display && IsWindowLocked)
                RemoveWindowLock();
        }
        
        private static void Reset()
        {
            _selectedFolder = null;
            _drawUploadScreen = false;
        }
    }
}
