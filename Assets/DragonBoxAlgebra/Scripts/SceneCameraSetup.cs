using UnityEngine;

namespace DragonBoxAlgebra
{
    /// <summary>
    /// Keeps exactly one Main Camera and one AudioListener in the scene.
    /// </summary>
    public static class SceneCameraSetup
    {
        public static void EnsureSingleMainCamera()
        {
            Camera[] cameras = Object.FindObjectsOfType<Camera>();
            Camera main = Camera.main;

            if (main == null && cameras.Length > 0)
            {
                foreach (Camera camera in cameras)
                {
                    if (camera.gameObject.name == "Main Camera")
                    {
                        camera.gameObject.tag = "MainCamera";
                        main = camera;
                        break;
                    }
                }

                if (main == null)
                {
                    cameras[0].gameObject.tag = "MainCamera";
                    main = cameras[0];
                }
            }

            if (main != null)
            {
                foreach (Camera camera in cameras)
                {
                    if (camera == main)
                    {
                        continue;
                    }

                    DestroyObject(camera.gameObject);
                }

                ConfigureMainCamera(main);
                EnsureExactlyOneAudioListener(main.gameObject);
                return;
            }

            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            ConfigureMainCamera(cameraGo.AddComponent<Camera>());
            cameraGo.AddComponent<AudioListener>();
        }

        private static void ConfigureMainCamera(Camera camera)
        {
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.34f, 0.42f);
        }

        private static void EnsureExactlyOneAudioListener(GameObject mainCameraGo)
        {
            AudioListener keep = mainCameraGo.GetComponent<AudioListener>();
            if (keep == null)
            {
                keep = mainCameraGo.AddComponent<AudioListener>();
            }

            AudioListener[] listeners = Object.FindObjectsOfType<AudioListener>();
            foreach (AudioListener listener in listeners)
            {
                if (listener == keep)
                {
                    continue;
                }

                DestroyObject(listener);
            }
        }

        private static void DestroyObject(Object target)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
