namespace LunaClient.Base.Interface
{
    /// <summary>
    /// System base interface
    /// </summary>
    public interface ISystem
    {
        bool Enabled { get; set; }
        void Update();
        void FixedUpdate();
        string GetProfilersData();
        void ResetProfilers();
    }
}