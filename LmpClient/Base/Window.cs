using LmpClient.Base.Interface;
using LmpClient.Systems.SettingsSys;
using System;
using UnityEngine;

namespace LmpClient.Base
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

        private string Tooltip { get; set; }
        private readonly GUIContent _tipContent = new GUIContent();
        private double _tipTime;

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

        public void OnGui()
        {
            if (!Initialized)
            {
                InitializeStyles();
                
                //We only set the styles once so we shouldn't worry so much about the memory footprint...
                SetStyles();

                Initialized = true;
            }

            //Always check the window locks
            CheckWindowLock();

            if (!Display)
            {
                return;
            }
            
            //Use our standard skin for rendering.
            GUI.skin = Skin;

            //Delegate to children
            DrawGui();
        }

        protected abstract void DrawGui();

        public virtual void CheckWindowLock()
        {
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

            if (!string.IsNullOrEmpty(Tooltip))
            {
                //Render it if hovering long enough.
                if (Time.unscaledTime - _tipTime > 0.35f)
                {
                    _tipContent.text = Tooltip;
                    var size = StyleLibrary.ToolTipStyle.CalcSize(_tipContent);
                    size.x += 8;
                    size.y += 4;

                    var rect = GUIUtility.ScreenToGUIRect(new Rect(Mouse.screenPos.x, Mouse.screenPos.y - size.y,
                        size.x, size.y));

                    GUILayout.BeginArea(rect);
                    GUILayout.Label(Tooltip, ToolTipStyle);
                    GUILayout.EndArea();
                }
            }
            else
            {
                _tipTime = Time.unscaledTime;
            }            
            
            //Collect the Tooltip, if any, in the Paint Event that follows the Layout events.
            //We do this here so we DO NOT change our layout between the two events.
            if (Event.current.type == EventType.Repaint)
            {
                Tooltip = GUI.tooltip;
            }
        }


        protected abstract void DrawWindowContent(int windowId);

        protected virtual void OnCloseButton()
        {
            Display = false;
        }

        protected void DrawCloseButton(Action closeAction, Rect rect)
        {
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;

            if (GUI.Button(new Rect(rect.width - 25, 4, 20, 20), CloseIcon, CloseButtonStyle))
            {
                closeAction.Invoke();                
            }

            GUI.backgroundColor = prev;
        }

        protected void DrawRefreshButton(Action refreshAction)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(RefreshIcon)) refreshAction.Invoke();
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
