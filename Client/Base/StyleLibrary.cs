using LunaClient.Utilities;
using LunaClient.Windows;
using UnityEngine;

namespace LunaClient.Base
{
    public abstract class StyleLibrary
    {
        protected Rect WindowRect;
        protected Rect MoveRect;

        protected GUIStyle WindowStyle;
        protected GUIStyle ButtonStyle;
        protected GUIStyle RedFontButtonStyle;
        protected GUIStyle SmallButtonStyle;
        protected GUIStyle ResizeButtonStyle;
        protected GUIStyle LabelStyle;
        protected GUIStyle HyperlinkLabelStyle;
        protected GUIStyle BoldLabelStyle;
        protected GUIStyle ScrollStyle;
        protected GUIStyle TextAreaStyle;
        protected GUIStyle StatusStyle;
        protected GUIStyle HighlightStyle;
        protected GUIStyle BoxStyle;
        protected GUIStyle BigLabelStyle;

        protected GUILayoutOption[] LayoutOptions;
        protected GUILayoutOption[] TextAreaOptions;
        protected GUILayoutOption[] LabelOptions;

        protected Vector2 ScrollPos = new Vector2();
        protected Texture2D SettingsIcon;
        protected Texture2D SettingsBigIcon;
        protected Texture2D ServerIcon;
        protected Texture2D ServerBigIcon;
        protected Texture2D SystemIcon;
        protected Texture2D ConnectIcon;
        protected Texture2D ConnectBigIcon;
        protected Texture2D DebugIcon;
        protected Texture2D DisconnectIcon;
        protected Texture2D DisconnectBigIcon;
        protected Texture2D LockIcon;
        protected Texture2D SyncIcon;
        protected Texture2D ResizeIcon;
        protected Texture2D CloseIcon;
        protected Texture2D RefreshIcon;
        protected Texture2D RefreshBigIcon;
        protected Texture2D UploadIcon;
        protected Texture2D DeleteIcon;
        protected Texture2D PlusIcon;
        protected Texture2D SaveIcon;
        protected Texture2D WaitIcon;
        protected Texture2D WaitGiantIcon;
        protected Texture2D SmallWaitIcon;
        protected Texture2D KeyIcon;
        protected Texture2D ChatIcon;
        protected Texture2D ChatRedIcon;
        protected Texture2D CameraIcon;
        protected Texture2D CameraRedIcon;
        protected Texture2D RocketIcon;
        protected Texture2D RocketRedIcon;
        protected Texture2D AdminIcon;
        protected Texture2D KickIcon;
        protected Texture2D KickBigIcon;
        protected Texture2D BanIcon;
        protected Texture2D BanBigIcon;
        protected Texture2D DekesslerIcon;
        protected Texture2D NukeIcon;
        protected Texture2D DekesslerBigIcon;
        protected Texture2D NukeBigIcon;

        public void InitializeStyles()
        {
            SettingsIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "settings.png"), 16, 16);
            SettingsBigIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "settingsBig.png"), 32, 32);
            ServerIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "server.png"), 16, 16);
            ServerBigIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "serverBig.png"), 32, 32);
            SystemIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "system.png"), 16, 16);
            ConnectIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "connect.png"), 16, 16);
            ConnectBigIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "connectBig.png"), 32, 32);
            DebugIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "debug.png"), 16, 16);
            DisconnectIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "disconnect.png"), 16, 16);
            DisconnectBigIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "disconnectBig.png"), 32, 32);
            LockIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "lock.png"), 16, 16);
            SyncIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "sync.png"), 16, 16);
            ResizeIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "resize.png"), 16, 16);
            CloseIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "close_small.png"), 10, 10);
            RefreshIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "refresh.png"), 16, 16);
            RefreshBigIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "refreshBig.png"), 32, 32);
            UploadIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "upload.png"), 16, 16);
            DeleteIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "delete.png"), 16, 16);
            PlusIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "plus.png"), 16, 16);
            SaveIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "save.png"), 16, 16);
            WaitGiantIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "waitGiant.png"), 16, 16);
            WaitIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "wait.png"), 16, 16);
            KeyIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "key.png"), 16, 16);
            ChatIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "chatWhite.png"), 16, 16);
            ChatRedIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "chatRed.png"), 16, 16);
            CameraIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "camera.png"), 16, 16);
            CameraRedIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "cameraRed.png"), 16, 16);
            RocketIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "rocket.png"), 16, 16);
            RocketRedIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "rocketRed.png"), 16, 16);
            AdminIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "admin.png"), 16, 16);
            KickIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "kick.png"), 16, 16);
            KickBigIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "kickBig.png"), 32, 32);
            BanIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "ban.png"), 16, 16);
            BanBigIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "banBig.png"), 32, 32);
            DekesslerIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "dekessler.png"), 16, 16);
            NukeIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "nuke.png"), 16, 16);
            DekesslerBigIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "dekesslerBig.png"), 32, 32);
            NukeBigIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "nukeBig.png"), 32, 32);

            WindowStyle = new GUIStyle(GUI.skin.window);
            ButtonStyle = new GUIStyle(GUI.skin.button);
            RedFontButtonStyle = new GUIStyle(GUI.skin.button) { normal = { textColor = Color.red }, active = { textColor = Color.red }, hover = { textColor = Color.red } };
            SmallButtonStyle = new GUIStyle(GUI.skin.button) { padding = new RectOffset(0, 0, 0, 0) };
            ResizeButtonStyle = new GUIStyle(GUI.skin.button) {
                padding = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0,0,0,0),
                normal = new GUIStyleState {background = ResizeIcon},
                active = new GUIStyleState { background = ResizeIcon },
                focused = new GUIStyleState { background = ResizeIcon },
                hover = new GUIStyleState { background = ResizeIcon },
                onNormal = new GUIStyleState { background = ResizeIcon },
                onActive = new GUIStyleState { background = ResizeIcon },
                onFocused = new GUIStyleState { background = ResizeIcon },
                onHover = new GUIStyleState { background = ResizeIcon },
            };
            LabelStyle = new GUIStyle(GUI.skin.label);
            HyperlinkLabelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, normal = new GUIStyleState { textColor = Color.blue } };
            BoldLabelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, normal = new GUIStyleState { textColor = Color.red } };
            ScrollStyle = new GUIStyle(GUI.skin.scrollView);
            TextAreaStyle = new GUIStyle(GUI.skin.textArea);
            BoxStyle = new GUIStyle(GUI.skin.box);
            BigLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 80,
                normal = { textColor = Color.red }
            };
        }
    }
}
