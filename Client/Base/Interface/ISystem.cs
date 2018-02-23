namespace LunaClient.Base.Interface
{
    /// <summary>
    /// System base interface
    /// </summary>
    public interface ISystem
    {
        string SystemName { get; }
        int ExecutionOrder { get; }
        bool Enabled { get; set; }
        void FixedUpdate();
        void Update();
        void LateUpdate();
    }
}