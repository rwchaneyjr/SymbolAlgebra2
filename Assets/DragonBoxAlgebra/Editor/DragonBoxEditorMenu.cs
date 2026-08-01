#if UNITY_EDITOR
using DragonBoxAlgebra.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DragonBoxAlgebra.Editor
{
    public static class DragonBoxEditorMenu
    {
        private const string ScenePath = "Assets/DragonBoxAlgebra/Scenes/DragonBox.unity";

        [MenuItem("DragonBox Algebra/Open Game Scene and Setup", false, 0)]
        public static void OpenAndSetup()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(ScenePath);
            SetupScene();
        }

        [MenuItem("DragonBox Algebra/Print Sprite Debug to Console", false, 30)]
        public static void PrintSpriteDebug()
        {
            CardSpriteLoader.Reset();
            CardSpriteLoader.EnsureLoaded();
            CreatureSpriteDebug.LogStartup();
        }

        [MenuItem("DragonBox Algebra/Fix Duplicate Cameras / Audio Listeners", false, 2)]
        public static void FixDuplicateCameras()
        {
            SceneCameraSetup.EnsureSingleMainCamera();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("DragonBox Algebra: Removed extra cameras/audio listeners. Keep only Main Camera.");
        }

        [MenuItem("DragonBox Algebra/Setup Scene (Camera + Bootstrap)", false, 1)]
        public static void SetupScene()
        {
            SceneCameraSetup.EnsureSingleMainCamera();
            EnsureEventSystem();
            EnsureAlgebraBootstrap();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(
                "DragonBox Algebra: Scene ready. Hierarchy should show AlgebraBootstrap, Main Camera, EventSystem. Press Play.");
        }

        private static void EnsureAlgebraBootstrap()
        {
            if (Object.FindObjectOfType<AlgebraBootstrap>() != null)
            {
                return;
            }

            var go = new GameObject("AlgebraBootstrap");
            go.AddComponent<AlgebraBootstrap>();
            Undo.RegisterCreatedObjectUndo(go, "Create AlgebraBootstrap");
            Selection.activeGameObject = go;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var eventGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(eventGo, "Create EventSystem");
        }
    }
}
#endif
