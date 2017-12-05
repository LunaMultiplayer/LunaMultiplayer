using LMP.Server.Client;
using LMP.Server.Command.Command.Base;
using LMP.Server.Command.Common;
using LMP.Server.Log;
using LMP.Server.Server;
using System.Net;

namespace LMP.Server.Command.Command
{
    public class BanIpCommand : HandledCommand
    {
        protected override string FileName => "LMPIPBans.txt";
        protected override object CommandLock { get; } = new object();

        public override void Execute(string commandArgs)
        {
            CommandSystemHelperMethods.SplitCommand(commandArgs, out var ip, out var reason);
            reason = string.IsNullOrEmpty(reason) ? "No reason specified" : reason;

            if (IPAddress.TryParse(ip, out var ipAddress))
            {
                var player = ClientRetriever.GetClientByIp(ipAddress);

                if (player != null)
                    MessageQueuer.SendConnectionEnd(player, $"You were banned from the server: {reason}");

                Add(ipAddress.ToString());
                LunaLog.Normal($"IP Address '{ip}' was banned from the server: {reason}");
            }
            else
            {
                LunaLog.Normal($"{ip} is not a valid IP address");
            }
        }
    }
}