using DragonBoxAlgebra.Audio;
using DragonBoxAlgebra.Gameplay;
using DragonBoxAlgebra.UI;
using UnityEngine;

namespace DragonBoxAlgebra
{
    public class AlgebraBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            SceneCameraSetup.EnsureSingleMainCamera();

            if (AudioManager.Instance == null)
            {
                var audioGo = new GameObject("AudioManager", typeof(AudioSource), typeof(AudioManager));
                DontDestroyOnLoad(audioGo);
            }

            CardSpriteLoader.Reset();
            CardSpriteLoader.EnsureLoaded();
            CreatureSpriteDebug.LogStartup();

            var controller = new AlgebraGameController();
            var ui = gameObject.AddComponent<AlgebraUI>();
            ui.Initialize(controller);
        }
    }
}
