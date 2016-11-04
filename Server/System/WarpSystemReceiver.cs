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
            LunaLog.Debug($"{client.PlayerName} created a new subspace. Id {WarpContext.NextSubspaceId}");

            //Create Subspace
            WarpContext.Subspaces.TryAdd(WarpContext.NextSubspaceId, message.ServerTimeDifference);
            VesselRelaySystem.CreateNewSubspace(WarpContext.NextSubspaceId);

            //Tell all Clients about the new Subspace
            var newMessageData = new WarpNewSubspaceMsgData
            {
                ServerTimeDifference = message.ServerTimeDifference,
                PlayerCreator = message.PlayerCreator,
                SubspaceKey = WarpContext.NextSubspaceId
            };
            MessageQueuer.SendToAllClients<WarpSrvMsg>(newMessageData);

            WarpSystem.SaveSubspace(WarpContext.NextSubspaceId, message.ServerTimeDifference); //Save to disk
            WarpContext.NextSubspaceId++;
        }

        public void HandleChangeSubspace(ClientStructure client, WarpChangeSubspaceMsgData message)
        {
            var oldSubspace = client.Subspace;
            var newSubspace = message.Subspace;

            if (oldSubspace != newSubspace)
            {
                MessageQueuer.RelayMessage<WarpSrvMsg>(client, new WarpChangeSubspaceMsgData
                {
                    PlayerName = client.PlayerName,
                    Subspace = message.Subspace
                });

                if (newSubspace != -1)
                {
                    client.Subspace = newSubspace;

                    if (client.Subspace != -1 && !ServerContext.Clients.Any(c => c.Value.Subspace == oldSubspace))
                    {
                        WarpSystem.RemoveSubspace(oldSubspace);
                        VesselRelaySystem.RemoveSubspace(oldSubspace);
                    }
                }
            }
        }
    }
}