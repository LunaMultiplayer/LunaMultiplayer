using LunaClient.Base.Interface;
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
            get => _display && MainSystem.ToolbarShowGui;
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
        public bool SafeDisplay { get; set; }
        public bool IsWindowLocked { get; set; }

        #endregion

        #region Methods

        public virtual void Update()
        {
            if (Display && Resizable)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    _resizingWindow = false;
                }

                if (_resizingWindow)
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

        public virtual void RemoveWindowLock()
        {
            //Implement your own code
        }

        /// <summary>
        /// Define here the style and components of your window
        /// </summary>
        public abstract void SetStyles();

        protected virtual bool Resizable { get; } = false;

        private bool _resizingWindow = false;

        protected void DrawContent(int windowId)
        {
            DrawCloseButton(OnCloseButton, WindowRect);
            if (Resizable)
            {
                if (GUI.RepeatButton(new Rect(WindowRect.width - 15, WindowRect.height - 15, 10, 10), ResizeIcon, ResizeButtonStyle))
                {
                    _resizingWindow = true;
                }
            }
            DrawWindowContent(windowId);
        }

        public abstract void DrawWindowContent(int windowId);

        protected virtual void OnCloseButton()
        {
            Display = false;
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
        
        protected void DrawWaitIcon()
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(WaitIcon);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        #endregion
    }
}
