using System;

// ReSharper disable All

#pragma warning disable IDE1006
namespace LmpUpdater.Appveyor.Contracts
{
    public class Job
    {
        public string jobId { get; set; }
        public string name { get; set; }
        public string osType { get; set; }
        public bool allowFailure { get; set; }
        public int messagesCount { get; set; }
        public int compilationMessagesCount { get; set; }
        public int compilationErrorsCount { get; set; }
        public int compilationWarningsCount { get; set; }
        public int testsCount { get; set; }
        public int passedTestsCount { get; set; }
        public int failedTestsCount { get; set; }
        public int artifactsCount { get; set; }
        public string status { get; set; }
        public DateTime started { get; set; }
        public DateTime finished { get; set; }
        public DateTime created { get; set; }
        public DateTime updated { get; set; }
    }
}
