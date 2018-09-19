using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Flag;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using System;
using System.Collections.Concurrent;

namespace LmpClient.Systems.Flag
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