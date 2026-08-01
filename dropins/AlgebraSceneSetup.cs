using DragonBoxAlgebra.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DragonBoxAlgebra
{
    /// <summary>
    /// Boots the game when the scene is empty or AlgebraBootstrap is missing.
    /// </summary>
    internal static class AlgebraSceneSetup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureScene()
        {
            SceneCameraSetup.EnsureSingleMainCamera();
            EnsureEventSystem();

            if (Object.FindObjectOfType<AlgebraBootstrap>() != null)
                return;

            if (Object.FindObjectOfType<AlgebraUI>() != null)
                return;

            var go = new GameObject("AlgebraBootstrap");
            go.AddComponent<AlgebraBootstrap>();
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
