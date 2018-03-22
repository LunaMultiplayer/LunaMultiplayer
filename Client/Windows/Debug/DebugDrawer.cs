using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using UniLinq;
using UnityEngine;

namespace LunaClient.Windows.Debug
{
    public partial class DebugWindow
    {
        public override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            _displayFast = GUILayout.Toggle(_displayFast, "Fast debug update", ButtonStyle);

            _displayVectors = GUILayout.Toggle(_displayVectors, "Display vessel vectors", ButtonStyle);
            if (_displayVectors)
                GUILayout.Label(_vectorText, LabelStyle);

            _displayOrbit = GUILayout.Toggle(_displayOrbit, "Display orbit info", ButtonStyle);
            if (_displayOrbit)
                GUILayout.Label(_orbitText, LabelStyle);

            _displayVesselStoreData = GUILayout.Toggle(_displayVesselStoreData, "Display vessel store data", ButtonStyle);
            if (_displayVesselStoreData)
                GUILayout.Label(_vesselStoreText, LabelStyle);

            _displayNtp = GUILayout.Toggle(_displayNtp, "Display NTP/Subspace statistics", ButtonStyle);
            if (_displayNtp)
                GUILayout.Label(_ntpText, LabelStyle);

            _displayConnectionQueue = GUILayout.Toggle(_displayConnectionQueue, "Display connection statistics", ButtonStyle);
            if (_displayConnectionQueue)
                GUILayout.Label(_connectionText, LabelStyle);

            if (GUILayout.Button("Reload all vessels", ButtonStyle))
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
                    vessel.vesselRanges = DebugUtils.PackRanges;
                }
            }
            
            if (GUILayout.Button("Unpack all vessels", ButtonStyle))
            {
                var vessels = VesselsProtoStore.AllPlayerVessels.Values.Select(v => v.Vessel).Where(v => v != null);
                foreach (var vessel in vessels)
                {
                    if (FlightGlobals.ActiveVessel?.id == vessel.id) continue;
                    vessel.vesselRanges = DebugUtils.UnPackRanges;
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
