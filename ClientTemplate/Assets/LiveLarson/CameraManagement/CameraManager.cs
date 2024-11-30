using System;
using UnityEngine;

namespace LiveLarson.CameraManagement
{
    public static class CameraManager
    {
        public enum CameraType
        {
            Main,
            UI
        }

        private static Camera _main;
        private static Camera _ui;
        private static Camera _3dui;

        public static Camera Main
        {
            get
            {
                if (_main == null) _main = Camera.main;
                if (_main == null) _main = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
                return _main;
            }
        }

        public static Camera UI
        {
            get
            {
                if (_ui == null)
                {
                    var cameraObject = GameObject.FindWithTag("UICamera");
                    if (cameraObject) _ui = cameraObject.GetComponent<Camera>();
                }

                return _ui;
            }
        }

        public static void Reset()
        {
            _main = null;
            _ui = null;
        }

        public static Camera Get(CameraType type)
        {
            return type switch
            {
                CameraType.Main => Main,
                CameraType.UI => UI,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}