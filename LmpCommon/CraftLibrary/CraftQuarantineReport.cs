using System.Collections.Generic;

namespace LmpCommon.CraftLibrary
{
    /// <summary>
    /// What a quarantine pass moved and what it could not handle.
    /// Callers log it, so client and server can run the same pass with their own loggers.
    /// </summary>
    public sealed class CraftQuarantineReport
    {
        private readonly List<QuarantinedCraft> _moved = new List<QuarantinedCraft>();
        private readonly List<QuarantineFailure> _failures = new List<QuarantineFailure>();

        public IReadOnlyList<QuarantinedCraft> Moved => _moved;

        public IReadOnlyList<QuarantineFailure> Failures => _failures;

        internal void AddMoved(string original, string quarantined)
        {
            _moved.Add(new QuarantinedCraft(original, quarantined));
        }

        internal void AddFailure(string path, string message)
        {
            _failures.Add(new QuarantineFailure(path, message));
        }
    }

    public sealed class QuarantinedCraft
    {
        public string Original { get; }

        public string Quarantined { get; }

        public QuarantinedCraft(string original, string quarantined)
        {
            Original = original;
            Quarantined = quarantined;
        }
    }

    public sealed class QuarantineFailure
    {
        public string Path { get; }

        public string Message { get; }

        public QuarantineFailure(string path, string message)
        {
            Path = path;
            Message = message;
        }
    }
}
