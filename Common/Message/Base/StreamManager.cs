namespace LunaCommon.Message.Base
{
    public class StreamManager
    {
        //Singleton
        public static RecyclableMemoryStreamManager MemoryStreamManager { get; } = new RecyclableMemoryStreamManager();
    }
}
