using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.System.VesselRelay;

namespace Server.System
{
    public class WarpSystemReceiver
    {
        private static readonly object CreateSubspaceLock = new object();

        public void HandleNewSubspace(ClientStructure client, WarpNewSubspaceMsgData message)
        {
            lock (CreateSubspaceLock)
            {
                if (message.PlayerCreator != client.PlayerName) return;

                LunaLog.Debug($"{client.PlayerName} created the new subspace '{WarpContext.NextSubspaceId}'");

                //Create Subspace
                WarpContext.Subspaces.TryAdd(WarpContext.NextSubspaceId, new Subspace(WarpContext.NextSubspaceId, message.ServerTimeDifference, client.PlayerName));
                VesselRelaySystem.CreateNewSubspace(WarpContext.NextSubspaceId);

                //Tell all Clients about the new Subspace
                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<WarpNewSubspaceMsgData>();
                msgData.ServerTimeDifference = message.ServerTimeDifference;
                msgData.PlayerCreator = message.PlayerCreator;
                msgData.SubspaceKey = WarpContext.NextSubspaceId;

                MessageQueuer.SendToAllClients<WarpSrvMsg>(msgData);
                WarpContext.NextSubspaceId++;
            }
        }

        public void HandleChangeSubspace(ClientStructure client, WarpChangeSubspaceMsgData message)
        {
            if (message.PlayerName != client.PlayerName) return;

            var oldSubspace = client.Subspace;
            var newSubspace = message.Subspace;

            if (oldSubspace != newSubspace)
            {
                if (newSubspace < 0)
                    LunaLog.Debug($"{client.PlayerName} is warping");
                else if (WarpContext.Subspaces[newSubspace].Creator != client.PlayerName)
                    LunaLog.Debug($"{client.PlayerName} synced with subspace '{message.Subspace}' created by {WarpContext.Subspaces[newSubspace].Creator}");

                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<WarpChangeSubspaceMsgData>();
                msgData.PlayerName = client.PlayerName;
                msgData.Subspace = message.Subspace;

                MessageQueuer.RelayMessage<WarpSrvMsg>(client, msgData);

                if (newSubspace != -1)
                {
                    client.Subspace = newSubspace;

                    //Try to remove his old subspace
                    if (WarpSystem.RemoveSubspace(oldSubspace))
                    {
                        VesselRelaySystem.RemoveSubspace(oldSubspace);
                    }
                }
            }
        }

        public void HandleSubspaceRequest(ClientStructure client)
        {
            lock (CreateSubspaceLock)
            {
                WarpSystemSender.SendAllSubspaces(client);
            }
        }
    }
}
