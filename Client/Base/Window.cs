using LunaClient.Base.Interface;
using LunaClient.Systems.SettingsSys;
using System;
using UnityEngine;

namespace LunaClient.Base
{
    /// <summary>
    /// Base class for windows.
    /// </summary>
    public abstract class Window<T> : StyleLibrary, IWindow
        where T : IWindow, new()
    {
        #region Fields

        public static T Singleton { get; } = new T();

        public string WindowName { get; } = typeof(T).Name;

        private bool _display;

        /// <summary>
        /// Set it to true or false to display this window
        /// </summary>
        public virtual bool Display
        {
            get => _display && SettingsSystem.CurrentSettings.DisclaimerAccepted;
            set
            {
                if (!_display && value)
                {
                    _display = true;
                    OnDisplay();
                }
                else if (_display && !value)
                {
                    _display = false;
                    OnHide();
                }
            }
        }

        /// <summary>
        /// Override to write code to execute when window is displayed
        /// </summary>
        public virtual void OnDisplay()
        {
            //Implement your own code
        }

        /// <summary>
        /// Override to write code to execute when window is hide
        /// </summary>
        public virtual void OnHide()
        {
            RemoveWindowLock();
            //Implement your own code
        }


        public bool Initialized { get; set; }
        public bool IsWindowLocked { get; set; }

        #endregion

        #region Methods

        public virtual void Update()
        {
            if (Display && Resizable)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    ResizingWindow = false;
                }

                if (ResizingWindow)
                {
                    WindowRect.width = Input.mousePosition.x - WindowRect.x + 10;
                    WindowRect.height = Screen.height - Input.mousePosition.y - WindowRect.y + 10;
                }
            }
            //Implement your own code
        }

        public virtual void OnGui()
        {
            if (!Initialized)
            {
                InitializeStyles();
                //We only set the styles once so we shouldn't worry so much about the memory footprint...
                SetStyles();
                Initialized = true;
            }
            //Implement your own code
        }

        public virtual void AfterGui()
        {
            if (Display && DisplayTooltips && !string.IsNullOrEmpty(Tooltip)) GUI.Label(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, Tooltip.Length * 10, 20), Tooltip);
        }

        public virtual void RemoveWindowLock()
        {
            //Implement your own code
        }

        /// <summary>
        /// Define here the style and components of your window
        /// </summary>
        public abstract void SetStyles();

        protected virtual bool Resizable { get; } = false;
        protected virtual bool DisplayTooltips { get; } = false;
        protected string Tooltip { get; set; } = string.Empty;

        protected bool ResizingWindow;
        protected void DrawContent(int windowId)
        {
            DrawCloseButton(OnCloseButton, WindowRect);
            if (Resizable)
            {
                if (GUI.RepeatButton(new Rect(WindowRect.width - 15, WindowRect.height - 15, 10, 10), ResizeIcon, ResizeButtonStyle))
                {
                    ResizingWindow = true;
                }
            }
            DrawWindowContent(windowId);
            SetTooltip();
        }
        
        public abstract void DrawWindowContent(int windowId);

        protected virtual void OnCloseButton()
        {
            Display = false;
        }

        protected void SetTooltip()
        {
            if (Event.current.type == EventType.Repaint) Tooltip = GUI.tooltip;
        }

        protected void DrawCloseButton(Action closeAction, Rect rect)
        {
            if (GUI.Button(new Rect(rect.width - 15, 4, 10, 10), CloseIcon, SmallButtonStyle))
                closeAction.Invoke();
        }

        protected void DrawRefreshButton(Action refreshAction)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(RefreshIcon, ButtonStyle)) refreshAction.Invoke();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        protected void DrawWaitIcon(bool small)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(small ? WaitIcon : WaitGiantIcon);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Call this method to prevent the window going offscreen
        /// </summary>
        protected Rect FixWindowPos(Rect inputRect)
        {
            //Let the user drag 3/4 of the window sideways off the screen
            var xMin = 0 - 3 / 4f * inputRect.width;
            var xMax = Screen.width - 1 / 4f * inputRect.width;

            //Don't let the title bar move above the top of the screen
            var yMin = 0;
            //Don't let the title bar move below the bottom of the screen
            float yMax = Screen.height - 20;

            if (inputRect.x < xMin)
                inputRect.x = xMin;
            if (inputRect.x > xMax)
                inputRect.x = xMax;
            if (inputRect.y < yMin)
                inputRect.y = yMin;
            if (inputRect.y > yMax)
                inputRect.y = yMax;

            return inputRect;
        }

        #endregion
    }
}
