namespace LmpClient.Base.Interface
{
    public interface IWindow
    {
        string WindowName { get; }

        void Update();
        void OnGui();

        void RemoveWindowLock();
        void CheckWindowLock();

        void SetStyles();
    }
}
