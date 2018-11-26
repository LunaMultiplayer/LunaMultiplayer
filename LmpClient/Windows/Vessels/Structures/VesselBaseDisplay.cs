using System.Text;
using UnityEngine;

namespace LmpClient.Windows.Vessels.Structures
{
    internal abstract class VesselBaseDisplay
    {
        protected static readonly GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button);
        protected static readonly StringBuilder StringBuilder = new StringBuilder();
        
        public void Print()
        {
            if (Display)
            {
                PrintDisplay();
            }
        }

        public void Update(Vessel vessel)
        {
            if (Display)
            {
                UpdateDisplay(vessel);
            }
        }

        protected abstract void PrintDisplay();
        protected abstract void UpdateDisplay(Vessel vessel);
        public abstract bool Display { get; set; }
    }
}
