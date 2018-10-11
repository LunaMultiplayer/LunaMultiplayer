using LmpClient.Localization;
using LmpClient.Utilities;
using LmpClient.Windows;
using UnityEngine;

namespace LmpClient.Base
{
    public abstract class StyleLibrary
    {
        protected GUISkin skin;
        
        //Custom styles used by LMP
        protected GUIStyle RedFontButtonStyle;
        protected GUIStyle SmallButtonStyle;
        protected GUIStyle ResizeButtonStyle;

        protected GUIStyle HyperlinkLabelStyle;
        protected GUIStyle BoldRedLabelStyle;
        protected GUIStyle BoldGreenLabelStyle;
        
        protected GUIStyle StatusStyle;
        protected GUIStyle BigLabelStyle;
        protected GUIStyle ToggleButtonStyle;

        protected GUILayoutOption[] LayoutOptions;
        protected GUILayoutOption[] TextAreaOptions;
        protected GUILayoutOption[] LabelOptions;
        
        
        //State, used by IMGUI Window Functions
        protected Rect WindowRect;
        protected Rect MoveRect;
        protected Vector2 ScrollPos = new Vector2();

        protected GUIContent SettingsIcon;
        protected GUIContent SettingsBigIcon;
        protected GUIContent ServerIcon;
        protected GUIContent ServerBigIcon;
        protected GUIContent SystemIcon;
        protected GUIContent ConnectIcon;
        protected GUIContent ConnectBigIcon;
        protected GUIContent DebugIcon;
        protected GUIContent DisconnectIcon;
        protected GUIContent DisconnectBigIcon;
        protected GUIContent LockIcon;
        protected GUIContent SyncIcon;
        protected Texture2D ResizeIcon;
        protected Texture2D CloseIcon;
        protected GUIContent RefreshIcon;
        protected GUIContent RefreshBigIcon;
        protected GUIContent UploadIcon;
        protected GUIContent DeleteIcon;
        protected GUIContent PlusIcon;
        protected GUIContent SaveIcon;
        protected Texture2D WaitIcon;
        protected Texture2D WaitGiantIcon;
        protected GUIContent KeyIcon;
        protected GUIContent GlobeIcon;
        protected GUIContent ChatIcon;
        protected GUIContent ChatRedIcon;
        protected GUIContent CameraIcon;
        protected GUIContent CameraRedIcon;
        protected GUIContent RocketIcon;
        protected GUIContent RocketRedIcon;
        protected GUIContent AdminIcon;
        protected GUIContent KickIcon;
        protected GUIContent KickBigIcon;
        protected GUIContent BanIcon;
        protected GUIContent BanBigIcon;
        protected GUIContent DekesslerIcon;
        protected GUIContent NukeIcon;
        protected GUIContent DekesslerBigIcon;
        protected GUIContent NukeBigIcon;
        protected GUIContent RestartServerIcon;
        protected GUIContent DownloadBigIcon;

        public void InitializeStyles()
        {            
            //We copy the original KSP skin, it's a ScriptableObject descendant.
            skin = Object.Instantiate(HighLogic.Skin);

            
            SettingsIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "settings.png"), 16, 16),
                LocalizationContainer.ButtonTooltips.SettingsIcon);
            SettingsBigIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "settingsBig.png"), 32, 32),
                LocalizationContainer.ButtonTooltips.SettingsIcon);
            ServerIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "server.png"), 16, 16),
                LocalizationContainer.ButtonTooltips.ServerIcon);
            ServerBigIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "serverBig.png"), 32, 32),
                LocalizationContainer.ButtonTooltips.ServerIcon);
            SystemIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "system.png"), 16, 16),
                "SYSTEM");
            ConnectIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "connect.png"), 16, 16),
                LocalizationContainer.ButtonTooltips.ConnectIcon);
            ConnectBigIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "connectBig.png"), 32, 32),
                LocalizationContainer.ButtonTooltips.ConnectIcon);
            DebugIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "debug.png"), 16, 16),
                "DEBUG");
            DisconnectIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "disconnect.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.DisconnectIcon);
            DisconnectBigIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "disconnectBig.png"), 32, 32), 
                LocalizationContainer.ButtonTooltips.DisconnectIcon);
            LockIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "lock.png"), 16, 16), 
                "LOCK");
            SyncIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "sync.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.SyncIcon);

            ResizeIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "resize.png"), 16, 16);
            CloseIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "close_small.png"), 10, 10);

            RefreshIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "refresh.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.RefreshIcon);
            RefreshBigIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "refreshBig.png"), 32, 32), 
                LocalizationContainer.ButtonTooltips.RefreshIcon);
            UploadIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "upload.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.UploadIcon);
            DeleteIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "delete.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.DeleteIcon);
            PlusIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "plus.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.PlusIcon);
            SaveIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "save.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.SaveIcon);

            WaitGiantIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "waitGiant.png"), 16, 16);
            WaitIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "wait.png"), 16, 16);

            KeyIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "key.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.KeyIcon);
            GlobeIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "globe.png"), 16, 16),
                LocalizationContainer.ButtonTooltips.KeyIcon);
            ChatIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "chatWhite.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.ChatIcon);
            ChatRedIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "chatRed.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.ChatIcon);
            CameraIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "camera.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.CameraIcon);
            CameraRedIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "cameraRed.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.CameraIcon);
            RocketIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "rocket.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.RocketIcon);
            RocketRedIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "rocketRed.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.RocketIcon);
            AdminIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "admin.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.AdminIcon);
            KickIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "kick.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.KickIcon);
            KickBigIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "kickBig.png"), 32, 32), 
                LocalizationContainer.ButtonTooltips.KickIcon);
            BanIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "ban.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.BanIcon);
            BanBigIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "banBig.png"), 32, 32), 
                LocalizationContainer.ButtonTooltips.BanIcon);
            DekesslerIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "dekessler.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.DekesslerIcon);
            NukeIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "nuke.png"), 16, 16), 
                LocalizationContainer.ButtonTooltips.NukeIcon);
            DekesslerBigIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "dekesslerBig.png"), 32, 32), 
                LocalizationContainer.ButtonTooltips.DekesslerIcon);
            NukeBigIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "nukeBig.png"), 32, 32), 
                LocalizationContainer.ButtonTooltips.NukeIcon);
            RestartServerIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "restartServerBig.png"), 32, 32),
                LocalizationContainer.ButtonTooltips.RestartServerIcon);
            DownloadBigIcon = new GUIContent(WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "downloadBig.png"), 32, 32),
                LocalizationContainer.ButtonTooltips.RestartServerIcon);
            
            RedFontButtonStyle = new GUIStyle(skin.button) { normal = { textColor = Color.red }, active = { textColor = Color.red }, hover = { textColor = Color.red } };
            SmallButtonStyle = new GUIStyle(skin.button) { padding = new RectOffset(0, 0, 0, 0) };
            ResizeButtonStyle = new GUIStyle(skin.button)
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
                       
            HyperlinkLabelStyle = new GUIStyle(skin.button)
            {
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0, 0, 0, 0),
                normal = new GUIStyleState { textColor = XKCDColors.KSPUnnamedCyan},
                active = new GUIStyleState(),
                focused = new GUIStyleState(),
                hover = new GUIStyleState(),
                onNormal = new GUIStyleState(),
                onActive = new GUIStyleState(),
                onFocused = new GUIStyleState(),
                onHover = new GUIStyleState()
            };
            
            BoldGreenLabelStyle = new GUIStyle(skin.label) { fontStyle = FontStyle.Bold, normal = new GUIStyleState { textColor = XKCDColors.KSPBadassGreen} };
            BoldRedLabelStyle = new GUIStyle(skin.label) { fontStyle = FontStyle.Bold, normal = new GUIStyleState { textColor = XKCDColors.KSPNotSoGoodOrange } };
            
            BigLabelStyle = new GUIStyle(skin.label)
            {
                fontSize = 80,
                normal = { textColor = Color.red }
            };
            
            //Custom style used by the "folder" type buttons.
            ToggleButtonStyle = skin.button;            
        }
    }
}
