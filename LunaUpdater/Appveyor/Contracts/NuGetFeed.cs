using System;
// ReSharper disable All

#pragma warning disable IDE1006
namespace LunaUpdater.Appveyor.Contracts
{
    public class NuGetFeed
    {
        public string id { get; set; }
        public string name { get; set; }
        public int accountId { get; set; }
        public int projectId { get; set; }
        public bool isPrivateProject { get; set; }
        public bool publishingEnabled { get; set; }
        public string accountTimeZoneId { get; set; }
        public DateTime created { get; set; }
    }
}
