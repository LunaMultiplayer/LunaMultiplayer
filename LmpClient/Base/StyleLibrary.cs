using LmpClient.Localization;
using LmpClient.Utilities;
using LmpClient.Windows;
using UnityEngine;

namespace LmpClient.Base
{
    public abstract class StyleLibrary
    {
        public static GUISkin DefaultSkin;

        //Simple flashing flag. 
        protected bool Flash => Time.time % 0.7f < 0.25f;

        protected const int TitleHeight = 30;

        protected static GUISkin Skin;

        //Custom styles used by LMP
        protected static GUIStyle RedFontButtonStyle;
        protected static GUIStyle CloseButtonStyle;
        protected static GUIStyle ResizeButtonStyle;

        protected static GUIStyle HyperlinkLabelStyle;
        protected static GUIStyle BoldRedLabelStyle;
        protected static GUIStyle BoldGreenLabelStyle;

        protected static GUIStyle StatusStyle;
        protected static GUIStyle BigLabelStyle;
        protected static GUIStyle ToggleButtonStyle;
        public static GUIStyle ToolTipStyle;

        protected GUILayoutOption[] LayoutOptions;
        protected GUILayoutOption[] TextAreaOptions;
        protected GUILayoutOption[] LabelOptions;


        //State, used by IMGUI Window Functions
        protected Rect WindowRect;
        protected Rect MoveRect;
        protected Vector2 ScrollPos = new Vector2();

        protected static GUIContent SettingsIcon;
        protected static GUIContent SettingsBigIcon;
        protected static GUIContent ServerIcon;
        protected static GUIContent ServerBigIcon;
        protected static GUIContent SystemIcon;
        protected static GUIContent ConnectIcon;
        protected static GUIContent ConnectBigIcon;
        protected static GUIContent DebugIcon;
        protected static GUIContent DisconnectIcon;
        protected static GUIContent DisconnectBigIcon;
        protected static GUIContent LockIcon;
        protected static GUIContent SyncIcon;
        protected static Texture2D ResizeIcon;
        protected static Texture2D CloseIcon;
        protected static GUIContent RefreshIcon;
        protected static GUIContent RefreshBigIcon;
        protected static GUIContent UploadIcon;
        protected static GUIContent DeleteIcon;
        protected static GUIContent PlusIcon;
        protected static GUIContent SaveIcon;
        protected static Texture2D WaitIcon;
        protected static Texture2D WaitGiantIcon;
        protected static GUIContent KeyIcon;
        protected static GUIContent GlobeIcon;
        protected static GUIContent ChatIcon;
        protected static GUIContent ChatRedIcon;
        protected static GUIContent CameraIcon;
        protected static GUIContent CameraRedIcon;
        protected static GUIContent RocketIcon;
        protected static GUIContent RocketRedIcon;
        protected static GUIContent AdminIcon;
        protected static GUIContent KickIcon;
        protected static GUIContent KickBigIcon;
        protected static GUIContent BanIcon;
        protected static GUIContent BanBigIcon;
        protected static GUIContent DekesslerIcon;
        protected static GUIContent NukeIcon;
        protected static GUIContent DekesslerBigIcon;
        protected static GUIContent NukeBigIcon;
        protected static GUIContent RestartServerIcon;
        protected static GUIContent DownloadBigIcon;
        protected static GUIContent CycleFirstIcon;
        protected static GUIContent CyclePreviousIcon;
        protected static GUIContent CycleNextIcon;
        protected static GUIContent CycleLastIcon;

        protected void InitializeStyles()
        {
            if (Skin == null)
            {
                //We copy the original KSP skin, it's a ScriptableObject descendant.
                Skin = Object.Instantiate(HighLogic.Skin);

                //Icons
                SettingsIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "settings.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.SettingsIcon);
                SettingsBigIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "settingsBig.png"), 32, 32),
                    LocalizationContainer.ButtonTooltips.SettingsIcon);
                ServerIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "server.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.ServerIcon);
                ServerBigIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "serverBig.png"), 32, 32),
                    LocalizationContainer.ButtonTooltips.ServerIcon);
                SystemIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "system.png"), 16, 16),
                    "SYSTEM");
                ConnectIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "connect.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.ConnectIcon);
                ConnectBigIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "connectBig.png"), 32, 32),
                    LocalizationContainer.ButtonTooltips.ConnectIcon);
                DebugIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "debug.png"), 16, 16),
                    "DEBUG");
                DisconnectIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "disconnect.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.DisconnectIcon);
                DisconnectBigIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "disconnectBig.png"), 32, 32),
                    LocalizationContainer.ButtonTooltips.DisconnectIcon);
                LockIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "lock.png"),
                        16, 16),
                    "LOCK");
                SyncIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "sync.png"),
                        16, 16),
                    LocalizationContainer.ButtonTooltips.SyncIcon);
                ResizeIcon =
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "resize.png"), 16, 16);
                CloseIcon = WindowUtil.LoadIcon(
                    CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                        "close_small.png"), 10, 10);
                RefreshIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "refresh.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.RefreshIcon);
                RefreshBigIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "refreshBig.png"), 32, 32),
                    LocalizationContainer.ButtonTooltips.RefreshIcon);
                UploadIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "upload.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.UploadIcon);
                DeleteIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "delete.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.DeleteIcon);
                PlusIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "plus.png"),
                        16, 16),
                    LocalizationContainer.ButtonTooltips.PlusIcon);
                SaveIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "save.png"),
                        16, 16),
                    LocalizationContainer.ButtonTooltips.SaveIcon);
                WaitGiantIcon =
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "waitGiant.png"), 16, 16);
                WaitIcon = WindowUtil.LoadIcon(
                    CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "wait.png"), 16,
                    16);
                KeyIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "key.png"),
                        16, 16),
                    LocalizationContainer.ButtonTooltips.KeyIcon);
                GlobeIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "globe.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.GlobeIcon);
                ChatIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "chatWhite.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.ChatIcon);
                ChatRedIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "chatRed.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.ChatIcon);
                CameraIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "camera.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.CameraIcon);
                CameraRedIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "cameraRed.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.CameraIcon);
                RocketIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "rocket.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.RocketIcon);
                RocketRedIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "rocketRed.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.RocketIcon);
                AdminIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "admin.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.AdminIcon);
                KickIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "kick.png"),
                        16, 16),
                    LocalizationContainer.ButtonTooltips.KickIcon);
                KickBigIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "kickBig.png"), 32, 32),
                    LocalizationContainer.ButtonTooltips.KickIcon);
                BanIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "ban.png"),
                        16, 16),
                    LocalizationContainer.ButtonTooltips.BanIcon);
                BanBigIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "banBig.png"), 32, 32),
                    LocalizationContainer.ButtonTooltips.BanIcon);
                DekesslerIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "dekessler.png"), 16, 16),
                    LocalizationContainer.ButtonTooltips.DekesslerIcon);
                NukeIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "nuke.png"),
                        16, 16),
                    LocalizationContainer.ButtonTooltips.NukeIcon);
                DekesslerBigIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "dekesslerBig.png"), 32, 32),
                    LocalizationContainer.ButtonTooltips.DekesslerIcon);
                NukeBigIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "nukeBig.png"), 32, 32),
                    LocalizationContainer.ButtonTooltips.NukeIcon);
                RestartServerIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "restartServerBig.png"), 32, 32),
                    LocalizationContainer.ButtonTooltips.RestartServerIcon);
                DownloadBigIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "downloadBig.png"), 32, 32),
                    LocalizationContainer.ButtonTooltips.DownloadIcon);
                CycleFirstIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "cycleFirstIcon.png"), 32, 32),
                    LocalizationContainer.ButtonTooltips.CycleFirstIcon);
                CyclePreviousIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "cyclePreviousIcon.png"), 32, 32),
                    LocalizationContainer.ButtonTooltips.CyclePreviousIcon);
                CycleNextIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "cycleNextIcon.png"), 32, 32),
                    LocalizationContainer.ButtonTooltips.CycleNextIcon);
                CycleLastIcon = new GUIContent(
                    WindowUtil.LoadIcon(
                        CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons",
                            "cycleLastIcon.png"), 32, 32),
                    LocalizationContainer.ButtonTooltips.CycleLastIcon);

                //Styles
                RedFontButtonStyle = new GUIStyle(Skin.button)
                {
                    normal = { textColor = Color.red },
                    active = { textColor = Color.red },
                    hover = { textColor = Color.red }
                };

                CloseButtonStyle = new GUIStyle(Skin.button) { padding = new RectOffset(2, 2, 2, 2), margin = new RectOffset(2, 2, 2, 2) };

                ResizeButtonStyle = new GUIStyle(Skin.button)
                {
                    padding = new RectOffset(0, 0, 0, 0),
                    border = new RectOffset(0, 0, 0, 0),
                    normal = new GUIStyleState { background = ResizeIcon },
                    active = new GUIStyleState { background = ResizeIcon },
                    focused = new GUIStyleState { background = ResizeIcon },
                    hover = new GUIStyleState { background = ResizeIcon },
                    onNormal = new GUIStyleState { background = ResizeIcon },
                    onActive = new GUIStyleState { background = ResizeIcon },
                    onFocused = new GUIStyleState { background = ResizeIcon },
                    onHover = new GUIStyleState { background = ResizeIcon }
                };

                HyperlinkLabelStyle = new GUIStyle(Skin.button)
                {
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset(0, 0, 0, 0),
                    border = new RectOffset(0, 0, 0, 0),
                    normal = new GUIStyleState { textColor = XKCDColors.KSPUnnamedCyan },
                    active = new GUIStyleState(),
                    focused = new GUIStyleState(),
                    hover = new GUIStyleState(),
                    onNormal = new GUIStyleState(),
                    onActive = new GUIStyleState(),
                    onFocused = new GUIStyleState(),
                    onHover = new GUIStyleState()
                };

                BoldGreenLabelStyle = new GUIStyle(Skin.label)
                {
                    fontStyle = FontStyle.Bold,
                    normal = new GUIStyleState { textColor = XKCDColors.KSPBadassGreen }
                };

                BoldRedLabelStyle = new GUIStyle(Skin.label)
                {
                    fontStyle = FontStyle.Bold,
                    normal = new GUIStyleState { textColor = XKCDColors.KSPNotSoGoodOrange }
                };

                BigLabelStyle = new GUIStyle(Skin.label)
                {
                    fontSize = 60,
                    normal = { textColor = XKCDColors.KSPNotSoGoodOrange }
                };

                ToolTipStyle = new GUIStyle(Skin.box)
                {
                    padding = new RectOffset(2, 2, 2, 2)
                };

                //Custom style used by the "folder" type buttons.
                ToggleButtonStyle = Skin.button;
            }
        }
    }
}
