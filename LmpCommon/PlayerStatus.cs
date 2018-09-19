namespace LmpCommon
{
    public class PlayerStatus
    {
        public string PlayerName { get; set; }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                DisplayText = !string.IsNullOrEmpty(_vesselText) ? $"{_statusText} ({_vesselText})" : _statusText;
            }
        }

        private string _vesselText;
        public string VesselText
        {
            get => _vesselText;
            set
            {
                _vesselText = value;
                DisplayText = !string.IsNullOrEmpty(_vesselText) ? $"{_statusText} ({_vesselText})" : _statusText;
            }
        }

        public string DisplayText { get; private set; }
    }
}
