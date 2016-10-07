using System;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Message.Reader.Base;
using LunaServer.System;

namespace LunaServer.Message.Reader
{
    public class LockSystemMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData message)
        {
            var data = (LockBaseMsgData) message;
            switch (data.LockMessageType)
            {
                case LockMessageType.LIST_REQUEST:
                    LockSystemSender.SendAllLocks(client);
                    break;
                case LockMessageType.ACQUIRE:
                    var acquireData = (LockAcquireMsgData) message;
                    if (acquireData.PlayerName == client.PlayerName)
                        LockSystemSender.SendLockAquireMessage(acquireData.LockName, acquireData.PlayerName,
                            acquireData.Force);
                    break;
                case LockMessageType.RELEASE:
                    var releaseData = (LockReleaseMsgData) message;
                    if (releaseData.PlayerName == client.PlayerName)
                        LockSystemSender.SendLockReleaseMessage(releaseData.LockName, releaseData.PlayerName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}