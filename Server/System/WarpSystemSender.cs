using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Server;
using System.Collections.Generic;
using System.Linq;

namespace LunaServer.System
{
    public class WarpSystemSender
    {
        public static void SendAllSubspaces(ClientStructure client)
        {
            LunaLog.Debug($"Sending {client.PlayerName} {WarpContext.Subspaces.Count} possible subspaces");

            MessageQueuer.SendToClient<WarpSrvMsg>(client, new WarpSubspacesReplyMsgData
            {
                SubspaceTime = WarpContext.Subspaces.Values.ToArray(),
                SubspaceKey = WarpContext.Subspaces.Keys.ToArray(),
                Players = ClientRetriever.GetAuthenticatedClients().Where(c => !Equals(c, client))
                    .Select(p => new KeyValuePair<int, string>(p.Subspace, p.PlayerName)).ToArray()
            });
        }
    }
}