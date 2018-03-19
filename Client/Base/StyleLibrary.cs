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

        protected GUILayoutOption[] LayoutOptions;
        protected GUILayoutOption[] TextAreaOptions;
        protected GUILayoutOption[] LabelOptions;

        protected Vector2 ScrollPos = new Vector2();
        protected Texture2D ResizeIcon;
        protected Texture2D CloseIcon;
        protected Texture2D RefreshIcon;
        protected Texture2D UploadIcon;
        protected Texture2D DeleteIcon;
        protected Texture2D SaveIcon;
        protected Texture2D WaitIcon;
        protected Texture2D SmallWaitIcon;

        public void InitializeStyles()
        {
            ResizeIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "resize.png"), 16, 16);
            CloseIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "close_small.png"), 10, 10);
            RefreshIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "refresh.png"), 16, 16);
            UploadIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "upload.png"), 16, 16);
            DeleteIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "delete.png"), 16, 16);
            SaveIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "save.png"), 16, 16);
            WaitIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "waitBig.png"), 16, 16);
            SmallWaitIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "waitSmall.png"), 16, 16);

            WindowStyle = new GUIStyle(GUI.skin.window);
            ButtonStyle = new GUIStyle(GUI.skin.button);
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
        }
    }
}
