namespace LunaClient.Base.Interface
{
    public interface IWindow
    {
        string WindowName { get; }
        void Update();
        void SafeUpdate();
        void OnGui();
        void RemoveWindowLock();
        void SetStyles();
    }
}