using LunaClient.Localization;
using LunaClient.Network;
using LunaClient.Windows.ServerList;
using UnityEngine;

namespace LunaClient.Windows.ServerDetails
{
    public partial class ServerDetailsWindow
    {
        public override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.ServerDetailsWindowText.Password, LabelOptions);
            NetworkServerList.Password = GUILayout.TextArea(NetworkServerList.Password, 30, TextAreaStyle, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(LocalizationContainer.ServerDetailsWindowText.Connect, ButtonStyle))
            {
                NetworkServerList.IntroduceToServer(ServerId);
                Display = false;
                ServerListWindow.Singleton.Display = false;
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(LocalizationContainer.ServerDetailsWindowText.Close, ButtonStyle))
            {
                Display = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}