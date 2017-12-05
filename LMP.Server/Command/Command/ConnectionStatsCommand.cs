using LMP.Server.Client;
using LMP.Server.Command.Command.Base;
using LMP.Server.Log;
using LunaCommon.Time;

namespace LMP.Server.Command.Command
{
    public class ConnectionStatsCommand : SimpleCommand
    {
        public override void Execute(string commandArgs)
        {
            //Do some shit here.
            long bytesSentTotal = 0;
            long bytesReceivedTotal = 0;
            LunaLog.Normal("Connection stats:");
            LunaLog.Normal($"Nist Time Difference: {LunaTime.TimeDifference.TotalMilliseconds} ms");
            foreach (var client in ClientRetriever.GetAuthenticatedClients())
            {
                bytesSentTotal += client.BytesSent;
                bytesReceivedTotal += client.BytesReceived;
                LunaLog.Normal(
                    $"Player '{client.PlayerName}', sent: {client.BytesSent}, received: {client.BytesReceived}");
            }
            LunaLog.Normal($"Server sent: {bytesSentTotal}, received: {bytesReceivedTotal}");
        }
    }
}