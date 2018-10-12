using LmpClient.Systems.Lock;
using UnityEngine;

namespace LmpClient.Windows.Locks
{
    public partial class LocksWindow
    {
        protected override void DrawWindowContent(int windowId)
        {
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            ScrollPos = GUILayout.BeginScrollView(ScrollPos, GUILayout.Width(WindowWidth), GUILayout.Height(WindowHeight));

            PrintLocks();

            GUILayout.EndScrollView();
        }

        private static void PrintLocks()
        {
            GUILayout.Label("Asteroid owner: " + _asteroidLockOwner);
            GUILayout.Label("Contract owner: " + _contractLockOwner);
            GUILayout.Space(10);
            for (var i = 0; i < VesselLocks.Count; i++)
            {
                GUILayout.BeginVertical(skin.box);
                VesselLocks[i].Selected = GUILayout.Toggle(VesselLocks[i].Selected, VesselLocks[i].VesselId.ToString());
                GUILayout.Label(VesselLocks[i].VesselName);
                
                if (VesselLocks[i].Selected)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(CreateLockText(VesselLocks[i]));
                    if (VesselLocks[i].PlayerOwnsAnyLock())
                    {
                        if (GUILayout.Button("Release"))
                        {
                            LockSystem.Singleton.ReleaseAllVesselLocks(null, VesselLocks[i].VesselId);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
        }

        private static string CreateLockText(VesselLockDisplay vesselLock)
        {
            StrBuilder.Length = 0;
            StrBuilder.Append("Loaded: ").Append(vesselLock.Loaded).AppendLine()
                .Append("Packed: ").Append(vesselLock.Packed).AppendLine()
                .Append("Immortal: ").Append(vesselLock.Immortal).AppendLine()
                .Append("Control: ").Append(vesselLock.ControlLockOwner).AppendLine()
                .Append("Update: ").Append(vesselLock.UpdateLockOwner).AppendLine()
                .Append("UnlUpdate: ").Append(vesselLock.UnloadedUpdateLockOwner);

            return StrBuilder.ToString();
        }
    }
}
