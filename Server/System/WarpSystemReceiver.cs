using System.Linq;
using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Server;

namespace LunaServer.System
{
    public class WarpSystemReceiver
    {
        public void HandleNewSubspace(ClientStructure client, WarpNewSubspaceMsgData message)
        {
            LunaLog.Debug("Create Subspace");

            //Create Subspace
            WarpContext.Subspaces.TryAdd(WarpContext.NextSubspaceId, message.SubspaceTimeDifference);

            //Tell all Clients about the new Subspace
            var newMessageData = new WarpNewSubspaceMsgData
            {
                SubspaceTimeDifference = message.SubspaceTimeDifference,
                PlayerCreator = message.PlayerCreator,
                SubspaceKey = WarpContext.NextSubspaceId
            };
            MessageQueuer.SendToAllClients<WarpSrvMsg>(newMessageData);

            WarpSystem.SaveSubspace(WarpContext.NextSubspaceId, message.SubspaceTimeDifference); //Save to disk
            WarpContext.NextSubspaceId++;
        }

        public void HandleChangeSubspace(ClientStructure client, WarpChangeSubspaceMsgData message)
        {
            var oldSubspace = client.Subspace;
            client.Subspace = message.Subspace;

            MessageQueuer.RelayMessage<WarpSrvMsg>(client, new WarpChangeSubspaceMsgData
            {
                PlayerName = client.PlayerName,
                Subspace = client.Subspace
            });

            if (!ServerContext.Clients.Any(c => c.Value.Subspace == oldSubspace))
            {
                WarpSystem.RemoveSubspace(oldSubspace);
            }
        }
    }
}