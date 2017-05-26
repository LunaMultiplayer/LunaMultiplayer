using UnityEngine;

namespace LunaClient.Utilities.DebugTools
{
    public class LineRendererDebug
    {
        private readonly GameObject _gameObject;
        private readonly LineRenderer _lineRenderer;

        public LineRendererDebug(Color lineColour)
        {
            //Texture
            var newTex = new Texture2D(1, 1);
            newTex.SetPixel(0, 0, lineColour);
            newTex.Apply();

            _gameObject = new GameObject();

            _lineRenderer = _gameObject.AddComponent<LineRenderer>();
            _lineRenderer.SetWidth(0.1f, 0.1f);
            _lineRenderer.SetVertexCount(2);
            _lineRenderer.SetColors(Color.red, Color.red);
            _lineRenderer.material = new Material(Shader.Find("Unlit/Texture")) { mainTexture = newTex };
        }

        public void UpdatePosition(Vector3 startPos, Vector3 endPos)
        {
            _lineRenderer.SetPosition(0, startPos);
            _lineRenderer.SetPosition(1, endPos);
        }

        public void Destroy()
        {
            Object.Destroy(_gameObject);
        }
    }
}