using System.Runtime.CompilerServices;

// LmpCommon used to be merged into LmpClient, so mods (e.g. LunaCompat) compiled against
// older LmpClient builds reference these types as living in LmpClient. Now that LmpCommon
// ships as its own assembly, these forwarders keep those mods binding correctly at runtime.
//
// This is the single source of type forwarders for LmpClient. Keep it deduplicated:
// declaring the same TypeForwardedTo in more than one place raises compiler error CS0739.

// Core utilities
[assembly: TypeForwardedTo(typeof(LmpCommon.PlayerStatus))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Common))]
[assembly: TypeForwardedTo(typeof(LmpCommon.LmpVersioning))]

// Message factories
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.ClientMessageFactory))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.ServerMessageFactory))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.MasterServerMessageFactory))]

// Core message structures
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Base.MessageBase<>))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Base.MessageData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Base.FactoryBase))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Base.MessageStore))]

// Client messages
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.Base.CliMsgBase<>))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.ModCliMsg))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.AdminCliMsg))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.ChatCliMsg))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.PlayerColorCliMsg))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.PlayerStatusCliMsg))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.VesselCliMsg))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.WarpCliMsg))]

// Message data types
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.ModMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselBaseMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselActionGroupMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselCoupleMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselDecoupleMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselFairingMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselFlightStateMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselPartSyncCallMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselPartSyncFieldMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselPartSyncUiFieldMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselPositionMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselProtoMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselRemoveMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselResourceMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselSyncMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselUndockMsgData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Data.Vessel.VesselUpdateMsgData))]

// Interfaces
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Interface.IMessageData))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Interface.IMessageBase))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Interface.IClientMessageBase))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Interface.IServerMessageBase))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Interface.IMasterServerMessageBase))]

// Enums
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Types.VesselMessageType))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Enums.ClientMessageType))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Enums.ServerMessageType))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Enums.MasterServerMessageType))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Enums.ClientState))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Enums.LogLevels))]
