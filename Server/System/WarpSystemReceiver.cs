using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.System.VesselRelay;
using System.Linq;

namespace Server.System
{
    public class WarpSystemReceiver
    {
        public void HandleNewSubspace(ClientStructure client, WarpNewSubspaceMsgData message)
        {
            if (message.PlayerCreator != client.PlayerName) return;

            LunaLog.Debug($"{client.PlayerName} created a new subspace. Id {WarpContext.NextSubspaceId}");

            //Create Subspace
            WarpContext.Subspaces.TryAdd(WarpContext.NextSubspaceId, message.ServerTimeDifference);
            VesselRelaySystem.CreateNewSubspace(WarpContext.NextSubspaceId);

            //Tell all Clients about the new Subspace
            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<WarpNewSubspaceMsgData>();
            msgData.ServerTimeDifference = message.ServerTimeDifference;
            msgData.PlayerCreator = message.PlayerCreator;
            msgData.SubspaceKey = WarpContext.NextSubspaceId;
            
            MessageQueuer.SendToAllClients<WarpSrvMsg>(msgData);
            WarpContext.NextSubspaceId++;
        }

        public void HandleChangeSubspace(ClientStructure client, WarpChangeSubspaceMsgData message)
        {
            if (message.PlayerName != client.PlayerName) return;

            var oldSubspace = client.Subspace;
            var newSubspace = message.Subspace;

            if (oldSubspace != newSubspace)
            {
                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<WarpChangeSubspaceMsgData>();
                msgData.PlayerName = client.PlayerName;
                msgData.Subspace = message.Subspace;

                MessageQueuer.RelayMessage<WarpSrvMsg>(client, msgData);

                if (newSubspace != -1)
                {
                    client.Subspace = newSubspace;

                    //If client stopped warping and there's nobody in that subspace, remove it
                    if (!ServerContext.Clients.Any(c => c.Value.Subspace == oldSubspace))
                    {
                        WarpSystem.RemoveSubspace(oldSubspace);
                        VesselRelaySystem.RemoveSubspace(oldSubspace);
                    }
                }
            }
        }
    }
}