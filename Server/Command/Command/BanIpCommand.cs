using System.Net;
using LunaServer.Client;
using LunaServer.Command.Command.Base;
using LunaServer.Command.Common;
using LunaServer.Log;
using LunaServer.Server;

namespace LunaServer.Command.Command
{
    public class BanIpCommand : HandledCommand
    {
        protected override string FileName => "LMPIPBans.txt";
        protected override object CommandLock { get; } = new object();

        public override void Execute(string commandArgs)
        {
            string ip;
            string reason;
            CommandSystemHelperMethods.SplitCommand(commandArgs, out ip, out reason);
            reason = string.IsNullOrEmpty(reason) ? "No reason specified" : reason;

            IPAddress ipAddress;
            if (IPAddress.TryParse(ip, out ipAddress))
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