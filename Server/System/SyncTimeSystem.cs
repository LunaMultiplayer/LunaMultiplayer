using System;
using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Interface;
using LunaServer.Client;
using LunaServer.Log;

namespace LunaServer.System
{
    public class SyncTimeSystem
    {
        //Write the send times down in SYNC_TIME_REPLY packets
        public static void RewriteMessage(ClientStructure client, IServerMessageBase message)
        {
            try
            {
                ((SyncTimeReplyMsgData) message.Data).ServerSendTime = DateTime.UtcNow.Ticks;
            }
            catch (Exception e)
            {
                LunaLog.Debug("Error rewriting SYNC_TIME packet, Exception " + e);
            }
        }
    }
}