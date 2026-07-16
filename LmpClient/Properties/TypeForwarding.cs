using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.ModCliMsg))]
[assembly: TypeForwardedTo(typeof(LmpCommon.PlayerStatus))]

// Forward other common client messages for broader mod compatibility
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.AdminCliMsg))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.ChatCliMsg))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.PlayerColorCliMsg))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.PlayerStatusCliMsg))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.VesselCliMsg))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.WarpCliMsg))]

// Forward message data types (often used by mods for custom data or hooking)
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.ModMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselBaseMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselProtoMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselUpdateMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselSyncMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselPositionMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselFlightStateMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselActionGroupMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselResourceMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselRemoveMsgData))]

// Forward message factories
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.ClientMessageFactory))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.ServerMessageFactory))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.MasterServerMessageFactory))]

// Forward core message structures
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Base.MessageBase<>))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Base.MessageData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Base.FactoryBase))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Base.MessageStore))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.Base.CliMsgBase<>))]

// Forward interfaces
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Interface.IMessageData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Interface.IMessageBase))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Interface.IClientMessageBase))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Interface.IServerMessageBase))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Interface.IMasterServerMessageBase))]

// Forward enums
[assembly: TypeForwardedTo(typeof(LmpCommon.Enums.ClientMessageType))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Enums.ServerMessageType))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Enums.MasterServerMessageType))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Enums.ClientState))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Enums.LogLevels))]

// Forward core utilities
[assembly: TypeForwardedTo(typeof(LmpCommon.LmpVersioning))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Common))]