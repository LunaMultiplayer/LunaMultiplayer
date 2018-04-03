namespace LunaClient.Base.Interface
{
    public interface IWindow
    {
        string WindowName { get; }
        void Update();
        void OnGui();
        void RemoveWindowLock();
        void SetStyles();
        void AfterGui();
    }
}
