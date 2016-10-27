using LunaClient.Base.Interface;

namespace LunaClient.Base
{    
    /// <summary>
     /// Base class for windows.
     /// </summary>
    public abstract class Window<T> : StyleLibrary, IWindow
        where T : class, IWindow, new()
    {
        #region Fields
        
        public virtual bool Display { get; set; }
        public bool Initialized { get; set; }
        public bool SafeDisplay { get; set; }
        public bool IsWindowLocked { get; set; }

        public static T Singleton { get; set; }

        #endregion

        static Window()
        {
            Singleton = new T();
        }

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
                SetStyles();
                Initialized = true;
            }
            //Implement your own code
        }

        public virtual void RemoveWindowLock()
        {
            //Implement your own code
        }

        public virtual void Reset()
        {
            RemoveWindowLock();
            //Implement your own code
        }

        public abstract void SetStyles();
        #endregion
    }
}