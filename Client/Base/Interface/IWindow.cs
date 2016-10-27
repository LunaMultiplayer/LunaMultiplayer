namespace LunaClient.Base.Interface
{
    public interface IWindow
    {
        void Update();
        void SafeUpdate();
        void OnGui();
        void RemoveWindowLock();
        void SetStyles();
    }
}