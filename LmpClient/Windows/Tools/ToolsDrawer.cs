using LmpClient.Systems.TimeSync;
using LmpClient.VesselUtilities;
using UniLinq;
using UnityEngine;

namespace LmpClient.Windows.Tools
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
                TimeSyncSystem.Singleton.ForceTimeSync();
            }

            ReloadSection();
            RangesSection();
            FloatingOriginSection();

            GUILayout.EndVertical();
        }

        private void RangesSection()
        {
            _displayRanges = GUILayout.Toggle(_displayRanges, "Vessel ranges", ButtonStyle);
            if (_displayRanges)
            {
                if (FlightGlobals.ActiveVessel != null)
                {
                    if (GUILayout.Button("Pack all vessels", ButtonStyle))
                    {
                        foreach (var vessel in FlightGlobals.Vessels.Where(v => v != null))
                        {
                            if (FlightGlobals.ActiveVessel.id == vessel.id) continue;
                            vessel.vesselRanges = ToolsUtils.PackRanges;
                        }
                    }

                    if (GUILayout.Button("Unpack all vessels", ButtonStyle))
                    {
                        foreach (var vessel in FlightGlobals.Vessels.Where(v => v != null))
                        {
                            if (FlightGlobals.ActiveVessel.id == vessel.id) continue;
                            vessel.vesselRanges = ToolsUtils.UnPackRanges;
                        }
                    }

                    if (GUILayout.Button("Reset ranges", ButtonStyle))
                    {
                        foreach (var vessel in FlightGlobals.Vessels.Where(v => v != null))
                        {
                            if (FlightGlobals.ActiveVessel.id == vessel.id) continue;
                            vessel.vesselRanges = PhysicsGlobals.Instance.VesselRangesDefault;
                        }
                    }
                }
            }
        }
        private void ReloadSection()
        {
            _displayReloads = GUILayout.Toggle(_displayReloads, "Reload vessels", ButtonStyle);
            if (_displayReloads)
            {
                if (GUILayout.Button("Reload own vessel", ButtonStyle))
                {
                    if (FlightGlobals.ActiveVessel != null)
                    {
                        FlightGlobals.ActiveVessel.protoVessel = FlightGlobals.ActiveVessel.BackupVessel();
                        VesselLoader.LoadVessel(FlightGlobals.ActiveVessel.protoVessel);
                    }
                }

                if (GUILayout.Button("Reload other vessels", ButtonStyle))
                {
                    var vessels = FlightGlobals.Vessels.Where(v => v != null).ToList();
                    if (FlightGlobals.ActiveVessel != null)
                    {
                        foreach (var vessel in vessels)
                        {
                            if (FlightGlobals.ActiveVessel.id == vessel.id) continue;
                            vessel.protoVessel = vessel.BackupVessel();
                            VesselLoader.LoadVessel(vessel.protoVessel);
                        }                        
                    }
                }
            }
        }
        
        private void FloatingOriginSection()
        {
            _displayFloatingOrigin = GUILayout.Toggle(_displayFloatingOrigin, "Floating Origin", ButtonStyle);
            if (_displayFloatingOrigin)
            {
                if (GUILayout.Button("Reset floating origin", ButtonStyle))
                {
                    FloatingOrigin.fetch.ResetOffset();
                }

                if (GUILayout.Button("Set random floating origin", ButtonStyle))
                {
                    FloatingOrigin.SetOffset(new Vector3d(Random.Range(0, FloatingOrigin.fetch.threshold), 
                        Random.Range(0, FloatingOrigin.fetch.threshold), 
                        Random.Range(0, FloatingOrigin.fetch.threshold)));
                }
            }
        }
    }
}
