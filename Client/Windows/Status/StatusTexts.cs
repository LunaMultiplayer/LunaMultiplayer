using LunaClient.Systems.Warp;
using LunaCommon;
using System.Text;

namespace LunaClient.Windows.Status
{
    /// <summary>
    /// This class is intended to store all the texts in the status window in the hope to reduce the GC allocations as otherwise the strings would be 
    /// created on every gui call
    /// </summary>
    public class StatusTexts
    {
        public const string DebugBtnTxt = "Debug";
        public const string SystemsBtnTxt = "Systems";
        public const string LocksBtnTxt = "Locks";
        public const string ToolsBtnTxt = "Tools";

        public const string WarpingLabelTxt = "WARPING";
        public const string Debug1BtnTxt = "D1";
        public const string Debug2BtnTxt = "D2";
        public const string Debug3BtnTxt = "D3";
        public const string Debug4BtnTxt = "D4";
        public const string Debug5BtnTxt = "D5";
        public const string Debug6BtnTxt = "D6";
        public const string Debug7BtnTxt = "D7";
        public const string Debug8BtnTxt = "D8";
        public const string Debug9BtnTxt = "D9";

        private const string TimePrefix = "T: ";
        private const string NegativeDeltaTimePrefix = " (-";
        private const string PositiveDeltaTimePrefix = " (+";
        private const string CloseDeltaTime = ")";

        private static readonly StringBuilder StringBuilder = new StringBuilder();
        
        public static string GetPlayerText(PlayerStatus playerStatus)
        {
            return playerStatus.VesselText;
        }

        public static string GetTimeLabel(SubspaceDisplayEntry currentEntry)
        {
            StringBuilder.Length = 0;

            var subspaceTime = WarpSystem.Singleton.GetSubspaceTime(currentEntry.SubspaceId);

            StringBuilder.Append(TimePrefix).Append(KSPUtil.PrintDateCompact(subspaceTime, true, true));

            if (WarpSystem.Singleton.CurrentSubspace != currentEntry.SubspaceId)
                AppendDeltaTime(subspaceTime);

            return StringBuilder.ToString();
        }

        private static void AppendDeltaTime(double subspaceTime)
        {
            var currentTime = Planetarium.GetUniversalTime();
            if (subspaceTime < currentTime)
            {
                StringBuilder.Append(NegativeDeltaTimePrefix).Append(KSPUtil.PrintTimeCompact(currentTime - subspaceTime, false));
            }
            else
            {
                StringBuilder.Append(PositiveDeltaTimePrefix).Append(KSPUtil.PrintTimeCompact(subspaceTime - currentTime, false));
            }

            StringBuilder.Append(CloseDeltaTime);
        }
    }
}
