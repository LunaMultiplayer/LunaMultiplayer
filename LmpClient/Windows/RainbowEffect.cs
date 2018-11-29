using UnityEngine;

namespace LmpClient.Windows
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class RainbowEffect : MonoBehaviour
    {
        private static readonly Color DefaultContentColor = GUI.contentColor;
        private static readonly Color[] Colors = { Color.red, Color.magenta, Color.blue, Color.cyan, Color.green, Color.yellow };

        private static Color _lerpedColor = Colors[0];

        private int _currentColorIndex = 0;
        private float _colorTime = 0;
        
        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        // ReSharper disable once InconsistentNaming
        public void OnGUI()
        {
            if (_colorTime <= 1)
            {
                _lerpedColor = Color.Lerp(Colors[_currentColorIndex], Colors[_currentColorIndex == Colors.Length - 1 ? 0 : _currentColorIndex + 1], _colorTime);
                _colorTime += 0.015f;
            }
            else
            {
                _colorTime = 0;

                if (_currentColorIndex == Colors.Length - 1)
                {
                    _currentColorIndex = 0;
                }
                else
                {
                    _currentColorIndex++;
                }
            }
        }

        public static void StartRainbowEffect()
        {
            GUI.contentColor = _lerpedColor;
        }

        public static void StopRainbowEffect()
        {
            GUI.contentColor = DefaultContentColor;
        }
    }
}
