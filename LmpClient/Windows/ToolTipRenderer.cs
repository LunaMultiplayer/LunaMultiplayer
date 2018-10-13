using System;
using LmpClient.Base;
using UnityEngine;

namespace LmpClient.Windows
{
    public class ToolTipRenderer : MonoBehaviour
    {
        private readonly GUIContent _tipContent = new GUIContent();
        private float _tipTime;


        private void OnGUI()
        {
            //If styles not initialized yet...
            if (StyleLibrary.ToolTipStyle == null) return;

            GUI.depth = -10;

            //We only care about painting events.
            if (Event.current.type != EventType.Repaint) return;

            //Draw Tooltip - DrawWindow will actually delegate and encapsulate the DrawContent(windowid) function in its own GUI Substate.
            //Therefore, we need to save it in DrawContent, and use it here.
            foreach (var window in WindowsHandler.Windows)
            {
                if (string.IsNullOrEmpty(window.Tooltip)) continue;

                //Render it if hovering long enough.
                if (Time.unscaledTime - _tipTime > 0.25f)
                {
                    _tipContent.text = window.Tooltip;
                    var size = StyleLibrary.ToolTipStyle.CalcSize(_tipContent);
                    size.x += 8;
                    size.y += 4;
                
                    GUI.Box(new Rect(Mouse.screenPos.x, Mouse.screenPos.y - size.y, size.x, size.y),
                        window.Tooltip, StyleLibrary.ToolTipStyle);                    
                }

                //Only process the first tip we find.
                return;
            }

            _tipTime = Time.unscaledTime;
        }
    }
}