using UnityEngine;

namespace LunaClient.Windows.Locks
{
    public partial class LocksWindow
    {
        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            ScrollPos = GUILayout.BeginScrollView(ScrollPos, GUILayout.Width(WindowWidth - 5), GUILayout.Height(WindowHeight - 100));

            PrintLocks();

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void PrintLocks()
        {
            GUILayout.Label("Asteroid owner: " + _asteroidLockOwner, LabelStyle);
            GUILayout.Space(10);
            for (var i = 0; i < VesselLocks.Count; i++)
            {
                VesselLocks[i].Selected = GUILayout.Toggle(VesselLocks[i].Selected, VesselLocks[i].VesselId.ToString(), ButtonStyle);
                if (VesselLocks[i].Selected)
                {
                    GUILayout.Label(CreateLockText(VesselLocks[i]), LabelStyle);
                }
            }
        }

        private static string CreateLockText(VesselLockDisplay vesselLock)
        {
            StrBuilder.Length = 0;
            StrBuilder.AppendLine(vesselLock.VesselName)
                .Append("Control: ").Append(vesselLock.ControlLockOwner).AppendLine()
                .Append("Update: ").Append(vesselLock.UpdateLockOwner).AppendLine()
                .Append("UnlUpdate: ").Append(vesselLock.UnloadedUpdateLockOwner).AppendLine()
                .Append("Exists in store: ").Append(vesselLock.ExistsInStore);

            return StrBuilder.ToString();
        }
    }
}
