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
            if (SceneExists(StartupConfig.BootstrapScenePath))
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(
                "Bootstrap Camera",
                new Vector3(0f, 8f, -8f),
                new Vector3(35f, 0f, 0f));

            EditorSceneManager.SaveScene(scene, StartupConfig.BootstrapScenePath);
        }

        private static void EnsureMvpArenaScene()
        {
            if (SceneExists(StartupConfig.MvpArenaScenePath))
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateDirectionalLight();
            CreateCamera(
                "Arena Camera",
                StartupConfig.ArenaCameraPosition,
                StartupConfig.ArenaCameraEulerAngles);
            CreateGround();

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

        private static void CreateDirectionalLight()
        {
            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateCamera(string objectName, Vector3 position, Vector3 eulerAngles)
        {
            GameObject cameraObject = new GameObject(objectName);
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = StartupConfig.CameraBackgroundColor;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;

            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = position;
            cameraObject.transform.eulerAngles = eulerAngles;
        }

        private static void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = StartupConfig.ArenaGroundPosition;
            ground.transform.localScale = StartupConfig.ArenaGroundScale;
        }
    }
}
#endif
