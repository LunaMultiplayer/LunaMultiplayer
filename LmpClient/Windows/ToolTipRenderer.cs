using LmpClient.Base;
using UnityEngine;

namespace LmpClient.Windows
{
    public class ToolTipRenderer : MonoBehaviour
    {
        public static string Tooltip = "";
        private readonly GUIContent _tipcontent = new GUIContent();
        private float _tipTime;


        private void OnGUI()
        {
            GUI.depth = -10;

            //Draw Tooltip - DrawWindow will actually delegate and encapsulate the DrawContent(windowid) function in its own GUI Substate.
            //Therefore, we need to save it in DrawContent, and use it here.
            if (Event.current.type == EventType.Repaint)
            {
                if (!string.IsNullOrEmpty(Tooltip))
                {
                    if (Time.time - _tipTime > 0.3f)
                    {
                        _tipcontent.text = Tooltip;
                        var size = StyleLibrary.ToolTipStyle.CalcSize(_tipcontent);
                        size.x += 8;
                        size.y += 4;                       
                        GUI.Box(new Rect(Mouse.screenPos.x,  Mouse.screenPos.y-size.y, size.x, size.y), Tooltip, StyleLibrary.ToolTipStyle);
                    }
                }
                else
                {
                    //No recent tooltip.
                    _tipTime = Time.time;
                }
            }
        }    
    }
}