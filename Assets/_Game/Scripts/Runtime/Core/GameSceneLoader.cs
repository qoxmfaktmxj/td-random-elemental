using UnityEngine;
using UnityEngine.SceneManagement;

namespace TdRandomElemental.Core
{
    public static class GameSceneLoader
    {
        public static bool LoadBootstrap()
        {
            return LoadScene(StartupConfig.BootstrapSceneName);
        }

        public static bool LoadMvpArena()
        {
            return LoadScene(StartupConfig.MvpArenaSceneName);
        }

        public static bool LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("Cannot load a scene with an empty name.");
                return false;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"Scene '{sceneName}' is not available. Check Build Settings.");
                return false;
            }

            if (SceneManager.GetActiveScene().name == sceneName)
            {
                return true;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            return true;
        }
    }
}
