using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Server;
using System.Linq;

namespace Server.System
{
    public class WarpSystemSender
    {
        public static void SendAllSubspaces(ClientStructure client)
        {
            LunaLog.Debug($"Sending {client.PlayerName} {WarpContext.Subspaces.Count} possible subspaces");

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<WarpSubspacesReplyMsgData>();

            var subspaces = WarpContext.Subspaces.ToArray().Select(s =>
            {
                var players = ClientRetriever.GetAuthenticatedClients()
                    .Where(c => !Equals(c, client) && c.Subspace == s.Key)
                    .Select(c=> c.PlayerName)
                    .ToArray();

                return new SubspaceInfo
                {
                    PlayerCount = players.Length,
                    Players = players,
                    SubspaceKey = s.Key,
                    SubspaceTime = s.Value
                };
            }).ToArray();


            msgData.Subspaces = subspaces;
            msgData.SubspaceCount = subspaces.Length;

            MessageQueuer.SendToClient<WarpSrvMsg>(client, msgData);
        }
    }
}