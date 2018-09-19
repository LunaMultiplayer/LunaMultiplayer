using LmpCommon.Time;
using Server.Client;
using Server.Command.Command.Base;
using Server.Log;

namespace Server.Command.Command
{
    public class ConnectionStatsCommand : SimpleCommand
    {
        public override bool Execute(string commandArgs)
        {
            //Do some shit here.
            long bytesSentTotal = 0;
            long bytesReceivedTotal = 0;
            LunaLog.Normal("Connection stats:");
            LunaLog.Normal($"Nist Time Difference: {LunaNetworkTime.TimeDifference.TotalMilliseconds} ms");
            foreach (var client in ClientRetriever.GetAuthenticatedClients())
            {
                bytesSentTotal += client.BytesSent;
                bytesReceivedTotal += client.BytesReceived;
                LunaLog.Normal(
                    $"Player '{client.PlayerName}', sent: {client.BytesSent}, received: {client.BytesReceived}");
            }
            LunaLog.Normal($"Server sent: {bytesSentTotal}, received: {bytesReceivedTotal}");

            return true;
        }
    }
}
