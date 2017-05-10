using LunaClient.Utilities;

namespace LunaClient.Base.Interface
{
    /// <summary>
    /// System base interface
    /// </summary>
    public interface ISystem
    {
        ProfilerData UpdateProfiler { get; }
        ProfilerData FixedUpdateProfiler { get; }
        bool Enabled { get; set; }
        void RunUpdate();
        void RunFixedUpdate();
    }
}