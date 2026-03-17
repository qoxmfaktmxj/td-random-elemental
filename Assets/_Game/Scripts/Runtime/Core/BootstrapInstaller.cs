using UnityEngine;
using UnityEngine.SceneManagement;

namespace TdRandomElemental.Core
{
    public static class BootstrapInstaller
    {
        private static bool _isLoadingStartupScene;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            _isLoadingStartupScene = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureStartupScene()
        {
            if (_isLoadingStartupScene)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != StartupConfig.BootstrapSceneName)
            {
                return;
            }

            _isLoadingStartupScene = true;
            bool didLoad = GameSceneLoader.LoadMvpArena();
            if (!didLoad)
            {
                _isLoadingStartupScene = false;
            }
        }
    }
}
