using LunaClient.Systems.TimeSyncer;
using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using UniLinq;
using UnityEngine;

namespace LunaClient.Windows.Tools
{
    public partial class ToolsWindow
    {
        public override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            var newVal = GUILayout.Toggle(_saveCurrentOrbitData, "Save orbit data to file", ButtonStyle);
            if (newVal != _saveCurrentOrbitData)
            {
                _saveCurrentOrbitData = newVal;
                if (newVal) CreateNewOrbitDataFile();
            }
            if (GUILayout.Button("Force time sync", ButtonStyle))
            {
                TimeSyncerSystem.Singleton.ForceTimeSync();
            }
            if (GUILayout.Button("Reload own vessel", ButtonStyle))
            {
                VesselLoader.ReloadVessel(FlightGlobals.ActiveVessel?.protoVessel);
            }
            if (GUILayout.Button("Reload other vessels", ButtonStyle))
            {
                var protos = VesselsProtoStore.AllPlayerVessels.Values.Select(v => v.ProtoVessel);
                foreach (var proto in protos)
                {
                    if (FlightGlobals.ActiveVessel?.id == proto.vesselID) continue;
                    VesselLoader.ReloadVessel(proto);
                }
            }
            if (GUILayout.Button("Pack all vessels", ButtonStyle))
            {
                var vessels = VesselsProtoStore.AllPlayerVessels.Values.Select(v => v.Vessel).Where(v => v != null);
                foreach (var vessel in vessels)
                {
                    if (FlightGlobals.ActiveVessel?.id == vessel.id) continue;
                    vessel.vesselRanges = ToolsUtils.PackRanges;
                }
            }
            if (GUILayout.Button("Unpack all vessels", ButtonStyle))
            {
                var vessels = VesselsProtoStore.AllPlayerVessels.Values.Select(v => v.Vessel).Where(v => v != null);
                foreach (var vessel in vessels)
                {
                    if (FlightGlobals.ActiveVessel?.id == vessel.id) continue;
                    vessel.vesselRanges = ToolsUtils.UnPackRanges;
                }
            }
            if (GUILayout.Button("Reset ranges", ButtonStyle))
            {
                var vessels = VesselsProtoStore.AllPlayerVessels.Values.Select(v => v.Vessel).Where(v => v != null);
                foreach (var vessel in vessels)
                {
                    if (FlightGlobals.ActiveVessel?.id == vessel.id) continue;
                    vessel.vesselRanges = PhysicsGlobals.Instance.VesselRangesDefault;
                }
            }

            GUILayout.EndVertical();
        }
    }
}
