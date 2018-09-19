using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LmpClient
{
    //Original author: Sarbian
    //Link: https://github.com/sarbian/DebugStuff/blob/master/DebugDrawer.cs

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class DebugDrawer : MonoBehaviour
    {
        private static readonly List<Line> Lines = new List<Line>();
        private static readonly List<Point> Points = new List<Point>();
        private static readonly List<Trans> Transforms = new List<Trans>();
        public Material LineMaterial;

        private struct Line
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly Color Color;

            public Line(Vector3 start, Vector3 end, Color color)
            {
                Start = start;
                End = end;
                Color = color;
            }
        }

        private struct Point
        {
            public readonly Vector3 Pos;
            public readonly Color Color;

            public Point(Vector3 pos, Color color)
            {
                Pos = pos;
                Color = color;
            }
        }

        private struct Trans
        {
            public readonly Vector3 Pos;
            public readonly Vector3 Up;
            public readonly Vector3 Right;
            public readonly Vector3 Forward;

            public Trans(Vector3 pos, Vector3 up, Vector3 right, Vector3 forward)
            {
                Pos = pos;
                Up = up;
                Right = right;
                Forward = forward;
            }
        }

        public static void DebugLine(Vector3 start, Vector3 end, Color col)
        {
            Lines.Add(new Line(start, end, col));
        }

        public static void DebugPoint(Vector3 start, Color col)
        {
            Points.Add(new Point(start, col));
        }

        public static void DebugTransforms(Transform t)
        {
            Transforms.Add(new Trans(t.position, t.up, t.right, t.forward));
        }

        private void Start()
        {
            DontDestroyOnLoad(this);
            if (!LineMaterial)
            {
                var shader = Shader.Find("Hidden/Internal-Colored");
                LineMaterial = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                LineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                LineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                LineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                LineMaterial.SetInt("_ZWrite", 0);
                LineMaterial.SetInt("_ZWrite", (int)UnityEngine.Rendering.CompareFunction.Always);
            }

            StartCoroutine(EndOfFrameDrawing());
        }

        private IEnumerator EndOfFrameDrawing()
        {
            Debug.Log("DebugDrawer starting");
            while (true)
            {
                yield return new WaitForEndOfFrame();

                var cam = GetActiveCam();

                if (cam == null) continue;

                try
                {
                    transform.position = Vector3.zero;

                    GL.PushMatrix();
                    LineMaterial.SetPass(0);

                    // In a modern Unity we would use cam.projectionMatrix.decomposeProjection to get the decomposed matrix
                    // and Matrix4x4.Frustum(FrustumPlanes frustumPlanes) to get a new one

                    // Change the far clip plane of the projection matrix
                    var projectionMatrix = Matrix4x4.Perspective(cam.fieldOfView, cam.aspect, cam.nearClipPlane, float.MaxValue);
                    GL.LoadProjectionMatrix(projectionMatrix);
                    GL.MultMatrix(cam.worldToCameraMatrix);
                    //GL.Viewport(new Rect(0, 0, Screen.width, Screen.height));

                    GL.Begin(GL.LINES);

                    foreach (var line in Lines)
                    {
                        DrawLine(line.Start, line.End, line.Color);
                    }

                    foreach (var point in Points)
                    {
                        DrawPoint(point.Pos, point.Color);
                    }

                    foreach (var t in Transforms)
                    {
                        DrawTransform(t.Pos, t.Up, t.Right, t.Forward);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("EndOfFrameDrawing Exception" + e);
                }
                finally
                {
                    GL.End();
                    GL.PopMatrix();

                    Lines.Clear();
                    Points.Clear();
                    Transforms.Clear();
                }
            }
        }

        private static Camera GetActiveCam()
        {
            if (!HighLogic.fetch)
                return Camera.main;

            if (HighLogic.LoadedSceneIsEditor && EditorLogic.fetch)
                return EditorLogic.fetch.editorCamera;

            if (HighLogic.LoadedSceneIsFlight && PlanetariumCamera.fetch && FlightCamera.fetch)
                return MapView.MapIsEnabled ? PlanetariumCamera.Camera : FlightCamera.fetch.mainCamera;

            return Camera.main;
        }

        private static void DrawLine(Vector3 origin, Vector3 destination, Color color)
        {
            GL.Color(color);
            GL.Vertex(origin);
            GL.Vertex(destination);
        }

        private static void DrawRay(Vector3 origin, Vector3 direction, Color color)
        {
            GL.Color(color);
            GL.Vertex(origin);
            GL.Vertex(origin + direction);
        }

        private static void DrawTransform(Vector3 position, Vector3 up, Vector3 right, Vector3 forward, float scale = 1.0f)
        {
            DrawRay(position, up * scale, Color.green);
            DrawRay(position, right * scale, Color.red);
            DrawRay(position, forward * scale, Color.blue);
        }

        private static void DrawPoint(Vector3 position, Color color, float scale = 1.0f)
        {
            DrawRay(position + Vector3.up * (scale * 0.5f), -Vector3.up * scale, color);
            DrawRay(position + Vector3.right * (scale * 0.5f), -Vector3.right * scale, color);
            DrawRay(position + Vector3.forward * (scale * 0.5f), -Vector3.forward * scale, color);
        }
    }
}
