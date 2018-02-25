using LunaClient.Base.Interface;

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
            //Implement your own code
        }

        public virtual void SafeUpdate()
        {
            //Implement your own code
        }

        public virtual void OnGui()
        {
            if (!Initialized)
            {
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

        #endregion
    }
}