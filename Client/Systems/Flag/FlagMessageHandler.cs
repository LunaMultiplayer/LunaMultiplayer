using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems.Flag
{
    public class FlagMessageHandler : SubSystem<FlagSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is FlagBaseMsgData msgData)) return;

            switch (msgData.FlagMessageType)
            {
                case FlagMessageType.ListResponse:
                    {
                        var data = (FlagListResponseMsgData)msgData;
                        for (var i = 0; i < data.FlagCount; i++)
                        {
                            var extendedFlagInfo = new ExtendedFlagInfo(data.FlagFiles[i]);
                            System.ServerFlags.TryAdd(extendedFlagInfo.FlagName, extendedFlagInfo);
                        }
                        MainSystem.NetworkState = ClientState.FlagsSynced;
                    }
                    break;
                case FlagMessageType.FlagData:
                    {
                        var data = (FlagDataMsgData)msgData;
                        var extendedFlagInfo = new ExtendedFlagInfo(data.Flag);
                        System.ServerFlags.AddOrUpdate(extendedFlagInfo.FlagName, extendedFlagInfo, (key, existingVal) => extendedFlagInfo);
                    }
                    break;
            }
        }
    }
}