using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Message.Reader.Base;
using LunaServer.System;
using System;

namespace LunaServer.Message.Reader
{
    public class LockSystemMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData message)
        {
            var data = (LockBaseMsgData)message;
            switch (data.LockMessageType)
            {
                case LockMessageType.ListRequest:
                    LockSystemSender.SendAllLocks(client);
                    break;
                case LockMessageType.Acquire:
                    var acquireData = (LockAcquireMsgData)message;
                    if (acquireData.Lock.PlayerName == client.PlayerName)
                        LockSystemSender.SendLockAquireMessage(acquireData.Lock, acquireData.Force);
                    break;
                case LockMessageType.Release:
                    var releaseData = (LockReleaseMsgData)message;
                    if (releaseData.Lock.PlayerName == client.PlayerName)
                        LockSystemSender.ReleaseAndSendLockReleaseMessage(releaseData.Lock);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}