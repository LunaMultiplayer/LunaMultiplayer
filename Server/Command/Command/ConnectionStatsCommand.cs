using LunaCommon.Time;
using LunaServer.Client;
using LunaServer.Command.Command.Base;
using LunaServer.Log;

namespace LunaServer.Command.Command
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