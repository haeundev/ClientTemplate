using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LiveLarson.Util.Editor
{
    public class ScreenshotTool : EditorWindow
    {
        private void OnGUI()
        {
            if (GUILayout.Button("Take Screenshot")) TakeScreenshot();
        }

        [MenuItem("Tools/Screenshot Tool")]
        public static void ShowWindow()
        {
            GetWindow<ScreenshotTool>("Screenshot Tool");
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            var e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.P) TakeScreenshot();
        }

        private static void TakeScreenshot()
        {
            var path = "Assets/Screenshots";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var screenshotName = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var fullPath = Path.Combine(path, screenshotName);
            ScreenCapture.CaptureScreenshot(fullPath);

            Debug.Log("Screenshot saved: " + fullPath);
        }
    }
}