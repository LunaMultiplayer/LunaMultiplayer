using UnityEngine;

namespace LunaClient.Utilities
{
    public class LmpGuiUtil
    {
        public const ControlTypes BlockAllControls = ControlTypes.ALLBUTCAMERAS ^ ControlTypes.MAP ^ ControlTypes.PAUSE ^ 
            ControlTypes.APPLAUNCHER_BUTTONS ^ ControlTypes.VESSEL_SWITCHING ^ ControlTypes.GUI;

        public static Rect PreventOffscreenWindow(Rect inputRect)
        {
            //Let the user drag 3/4 of the window sideways off the screen
            var xMin = 0 - 3/4f*inputRect.width;
            var xMax = Screen.width - 1/4f*inputRect.width;

            //Don't let the title bar move above the top of the screen
            var yMin = 0;
            //Don't let the title bar move below the bottom of the screen
            float yMax = Screen.height - 20;

            if (inputRect.x < xMin)
                inputRect.x = xMin;
            if (inputRect.x > xMax)
                inputRect.x = xMax;
            if (inputRect.y < yMin)
                inputRect.y = yMin;
            if (inputRect.y > yMax)
                inputRect.y = yMax;

            return inputRect;
        }
    }
}