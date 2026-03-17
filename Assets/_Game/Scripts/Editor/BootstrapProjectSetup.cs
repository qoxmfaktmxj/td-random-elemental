#if UNITY_EDITOR
using TdRandomElemental.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TdRandomElemental.Editor
{
    [InitializeOnLoad]
    public static class BootstrapProjectSetup
    {
        private static bool _isScheduled;

        static BootstrapProjectSetup()
        {
            ScheduleSetup();
        }

        private static void ScheduleSetup()
        {
            if (_isScheduled)
            {
                return;
            }

            _isScheduled = true;
            EditorApplication.delayCall += EnsureBootstrapProject;
        }

        private static void EnsureBootstrapProject()
        {
            _isScheduled = false;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating || Application.isPlaying)
            {
                ScheduleSetup();
                return;
            }

            RunProjectSetup();
        }

        public static void RunProjectSetup()
        {
            EnsureFolders();
            EnsureBootstrapScene();
            EnsureMvpArenaScene();
            EnsureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            foreach (string folderPath in StartupConfig.RequiredFolders)
            {
                EnsureFolder(folderPath);
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] segments = folderPath.Split('/');
            string currentPath = segments[0];
            for (int i = 1; i < segments.Length; i++)
            {
                string nextPath = currentPath + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, segments[i]);
                }

                currentPath = nextPath;
            }
        }

        private static void EnsureBootstrapScene()
        {
            Scene scene = OpenOrCreateScene(StartupConfig.BootstrapScenePath);
            EnsureCamera(
                "Bootstrap Camera",
                new Vector3(0f, 8f, -8f),
                new Vector3(35f, 0f, 0f));

            EditorSceneManager.SaveScene(scene, StartupConfig.BootstrapScenePath);
        }

        private static void EnsureMvpArenaScene()
        {
            Scene scene = OpenOrCreateScene(StartupConfig.MvpArenaScenePath);
            EnsureDirectionalLight();
            EnsureCamera(
                "Arena Camera",
                StartupConfig.ArenaCameraPosition,
                StartupConfig.ArenaCameraEulerAngles);
            EnsureGround();

            EditorSceneManager.SaveScene(scene, StartupConfig.MvpArenaScenePath);
        }

        private static void EnsureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(StartupConfig.BootstrapScenePath, true),
                new EditorBuildSettingsScene(StartupConfig.MvpArenaScenePath, true)
            };

            SceneAsset bootstrapScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(StartupConfig.BootstrapScenePath);
            if (bootstrapScene != null)
            {
                EditorSceneManager.playModeStartScene = bootstrapScene;
            }
        }

        private static bool SceneExists(string scenePath)
        {
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null;
        }

        private static Scene OpenOrCreateScene(string scenePath)
        {
            return SceneExists(scenePath)
                ? EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        private static void EnsureDirectionalLight()
        {
            GameObject lightObject = FindRootObject("Directional Light");
            if (lightObject == null)
            {
                lightObject = new GameObject("Directional Light");
            }

            Light light = lightObject.GetComponent<Light>();
            if (light == null)
            {
                light = lightObject.AddComponent<Light>();
            }

            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void EnsureCamera(string objectName, Vector3 position, Vector3 eulerAngles)
        {
            GameObject cameraObject = FindRootObject(objectName);
            if (cameraObject == null)
            {
                cameraObject = new GameObject(objectName);
            }

            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.GetComponent<Camera>();
            if (camera == null)
            {
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = StartupConfig.CameraBackgroundColor;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;

            if (cameraObject.GetComponent<AudioListener>() == null)
            {
                cameraObject.AddComponent<AudioListener>();
            }

            cameraObject.transform.position = position;
            cameraObject.transform.rotation = Quaternion.Euler(eulerAngles);
        }

        private static void EnsureGround()
        {
            GameObject ground = FindRootObject("Ground");
            if (ground == null)
            {
                ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ground.name = "Ground";
            }

            ground.transform.position = StartupConfig.ArenaGroundPosition;
            ground.transform.localScale = StartupConfig.ArenaGroundScale;
        }

        private static GameObject FindRootObject(string objectName)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            GameObject[] roots = activeScene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == objectName)
                {
                    return roots[i];
                }
            }

            return null;
        }
    }
}
#endif
