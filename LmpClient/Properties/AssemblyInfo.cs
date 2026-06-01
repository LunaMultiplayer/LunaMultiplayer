using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Luna Multiplayer Mod")]
[assembly: AssemblyDescription("Luna Multiplayer Mod (client)")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("LMP")]
[assembly: AssemblyCopyright("Copyright © 2018")]
[assembly: AssemblyTrademark("Gabriel Vazquez")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: Guid("cc8e38bb-d6d5-4bb9-ab74-a3a1a11ddc8d")]

[assembly: AssemblyVersion("0.29.3")]
[assembly: AssemblyFileVersion("0.29.3")]
[assembly: AssemblyInformationalVersion("0.29.3-compiled")]

[assembly: TypeForwardedTo(typeof(LmpCommon.PlayerStatus))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.ClientMessageFactory))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.MasterServerMessageFactory))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.ServerMessageFactory))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Client.ModCliMsg))]
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

[assembly: TypeForwardedTo(typeof(LmpCommon.Message.Types.VesselMessageType))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Enums.ClientMessageType))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Enums.MasterServerMessageType))]
[assembly: TypeForwardedTo(typeof(LmpCommon.Enums.ServerMessageType))]

[assembly: KSPAssembly("LMP", 0, 30)]