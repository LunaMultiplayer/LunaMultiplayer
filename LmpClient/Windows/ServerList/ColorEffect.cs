using LmpCommon;
using UnityEngine;

namespace LmpClient.Windows
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class ColorEffect : MonoBehaviour
    {
        private static Color _defaultContentColor;
        private static readonly Color[] Colors = { Color.red, Color.magenta, Color.blue, Color.cyan, Color.green, Color.yellow };

        private static Color _lerpedColor = Colors[0];

        private int _currentColorIndex = 0;
        private float _colorTime = 0;

        public void Awake()
        {
            _defaultContentColor = GUI.contentColor;
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

        public static void StartPaintingServer(ServerInfo server)
        {
            if (server.DedicatedServer)
            {
                if (server.RainbowEffect)
                    StartRainbowEffect();
                else
                    GUI.contentColor = new Color(server.Color[0] / 255f, server.Color[1] / 255f, server.Color[2] / 255f);
            }
        }

        public static void StopPaintingServer()
        {
            GUI.contentColor = _defaultContentColor;
        }

        private static void StartRainbowEffect()
        {
            GUI.contentColor = _lerpedColor;
        }
    }
}
