using LMP.Server.Client;
using LMP.Server.Context;
using LMP.Server.Log;
using LMP.Server.Server;
using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Server;
using System.Collections.Generic;
using System.Linq;

namespace LMP.Server.System
{
    public class WarpSystemSender
    {
        public static void SendAllSubspaces(ClientStructure client)
        {
            LunaLog.Debug($"Sending {client.PlayerName} {WarpContext.Subspaces.Count} possible subspaces");

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<WarpSubspacesReplyMsgData>();
            msgData.SubspaceTime = WarpContext.Subspaces.Values.ToArray();
            msgData.SubspaceKey = WarpContext.Subspaces.Keys.ToArray();
            msgData.Players = ClientRetriever.GetAuthenticatedClients().Where(c => !Equals(c, client))
                .Select(p => new KeyValuePair<int, string>(p.Subspace, p.PlayerName)).ToArray();

            MessageQueuer.SendToClient<WarpSrvMsg>(client, msgData);
        }
    }
}