using UnityEngine;

namespace LmpClient.Windows.Vessels
{
    public partial class VesselsWindow
    {
        protected override void DrawWindowContent(int windowId)
        {
            GUI.DragWindow(MoveRect);

            ScrollPos = GUILayout.BeginScrollView(ScrollPos, GUILayout.Width(WindowWidth), GUILayout.Height(WindowHeight));
            PrintVessels();
            GUILayout.EndScrollView();
        }

        private static void PrintVessels()
        {
            for (var i = 0; i < Vessels.Count; i++)
            {
                GUILayout.BeginVertical(Skin.box);
                Vessels[i].Selected = GUILayout.Toggle(Vessels[i].Selected, Vessels[i].VesselId.ToString());
                GUILayout.Label(Vessels[i].VesselName);
                
                if (Vessels[i].Selected)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(CreateVesselText(Vessels[i]));
                    switch (Vessels[i].ObtDriverMode)
                    {
                        case OrbitDriver.UpdateMode.TRACK_Phys:
                            if (GUILayout.Button("Set as update"))
                            {
                                FlightGlobals.FindVessel(Vessels[i].VesselId)?.orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.UPDATE);
                            }
                            break;
                        case OrbitDriver.UpdateMode.UPDATE:
                            if (GUILayout.Button("Set as track phys"))
                            {
                                FlightGlobals.FindVessel(Vessels[i].VesselId)?.orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.TRACK_Phys);
                            }
                            break;
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
        }

        private static string CreateVesselText(VesselDisplay vesselLock)
        {
            StrBuilder.Length = 0;
            StrBuilder.Append("Loaded: ").Append(vesselLock.Loaded).AppendLine()
                .Append("Packed: ").Append(vesselLock.Packed).AppendLine()
                .Append("Immortal: ").Append(vesselLock.Immortal).AppendLine()
                .Append("Obt DriverMode: ").Append(vesselLock.ObtDriverMode);

            return StrBuilder.ToString();
        }
    }
}
