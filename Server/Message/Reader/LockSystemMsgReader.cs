using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Message.Reader.Base;
using Server.System;
using System;

namespace Server.Message.Reader
{
    public class LockSystemMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = (LockBaseMsgData)message.Data;
            switch (data.LockMessageType)
            {
                case LockMessageType.ListRequest:
                    LockSystemSender.SendAllLocks(client);
                    //We don't use this message anymore so we can recycle it
                    message.Recycle();
                    break;
                case LockMessageType.Acquire:
                    var acquireData = (LockAcquireMsgData)data;
                    if (acquireData.Lock.PlayerName == client.PlayerName)
                        LockSystemSender.SendLockAquireMessage(client, acquireData.Lock, acquireData.Force);
                    break;
                case LockMessageType.Release:
                    var releaseData = (LockReleaseMsgData)data;
                    if (releaseData.Lock.PlayerName == client.PlayerName)
                        LockSystemSender.ReleaseAndSendLockReleaseMessage(client, releaseData.Lock);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}