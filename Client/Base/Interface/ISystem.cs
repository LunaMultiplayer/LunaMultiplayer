namespace LunaClient.Base.Interface
{
    /// <summary>
    /// System base interface
    /// </summary>
    public interface ISystem
    {
        bool Enabled { get; set; }
        void FixedUpdate();
        void Update();
        void LateUpdate();
        string GetProfilersData();
        void ResetProfilers();
    }
}