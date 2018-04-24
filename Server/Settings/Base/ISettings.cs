namespace Server.Settings.Base
{
    public interface ISettings
    {
        void Load();
        void Save();
    }
}
