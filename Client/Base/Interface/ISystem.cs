namespace LunaClient.Base.Interface
{
    /// <summary>
    /// System base interface
    /// </summary>
    public interface ISystem
    {
        bool Enabled { get; set; }
        void Reset();
        void Update();
        void FixedUpdate();
    }
}