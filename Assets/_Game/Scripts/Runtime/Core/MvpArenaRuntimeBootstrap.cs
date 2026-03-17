using TdRandomElemental.Board;
using TdRandomElemental.Character;
using TdRandomElemental.Enemies;
using TdRandomElemental.Summoning;
using TdRandomElemental.UI;
using TdRandomElemental.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TdRandomElemental.Core
{
    public static class MvpArenaRuntimeBootstrap
    {
        private const string RuntimePathLaneName = "__RuntimePathLane";
        private const string RuntimePlayerName = "__RuntimePlayer";
        private const string RuntimeWaveObjectName = "__WaveController";
        private const string RuntimeSummonServiceName = "__SummonService";
        private const string RuntimeSummonRequestHandlerName = "__SummonRequestHandler";
        private const string RuntimeTowerMergeServiceName = "__TowerMergeService";
        private const string RuntimeTowerSellServiceName = "__TowerSellService";
        private const string RuntimeRunFlowControllerName = "__RunFlowController";
        private const string RuntimeHudName = "__HudPresenter";

        private static readonly Vector3[] DefaultWaypointPositions =
        {
            new Vector3(-11f, 0f, 0f),
            new Vector3(-7f, 0f, 0f),
            new Vector3(-3f, 0f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(5f, 0f, 0f),
            new Vector3(9f, 0f, 0f),
            new Vector3(11f, 0f, 0f)
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoadHook()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != StartupConfig.MvpArenaSceneName)
            {
                return;
            }

            EnsureMvpArenaRuntime();
        }

        private static void EnsureMvpArenaRuntime()
        {
            EnsurePathLane();
            EnsureBoardService();
            EnsurePlayer();
            EnsureWaveController();
            EnsureSummonService();
            EnsureSummonRequestHandler();
            EnsureTowerMergeService();
            EnsureTowerSellService();
            EnsureRunFlowController();
            EnsureHudPresenter();
        }

        private static void EnsurePathLane()
        {
            if (Object.FindFirstObjectByType<PathLane>() != null)
            {
                return;
            }

            GameObject laneRoot = new GameObject(RuntimePathLaneName);

            for (int i = 0; i < DefaultWaypointPositions.Length; i++)
            {
                GameObject waypoint = new GameObject($"Waypoint_{i + 1:00}");
                waypoint.transform.SetParent(laneRoot.transform, false);
                waypoint.transform.position = DefaultWaypointPositions[i];
            }

            laneRoot.AddComponent<PathLane>();
        }

        private static void EnsureBoardService()
        {
            if (Object.FindFirstObjectByType<TowerBoardService>() != null)
            {
                return;
            }

            GameObject boardObject = new GameObject("__TowerBoardService");
            boardObject.AddComponent<TowerBoardService>();
        }

        private static void EnsurePlayer()
        {
            if (Object.FindFirstObjectByType<PlayerInputRouter>() != null)
            {
                return;
            }

            GameObject playerRoot = new GameObject(RuntimePlayerName);
            playerRoot.transform.position = new Vector3(0f, 0f, -6f);

            CharacterController controller = playerRoot.AddComponent<CharacterController>();
            controller.radius = 0.35f;
            controller.height = 2f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.stepOffset = 0.3f;

            playerRoot.AddComponent<PlayerInputRouter>();
            playerRoot.AddComponent<PlayerDashController>();
            playerRoot.AddComponent<PlayerInteractionSensor>();
            playerRoot.AddComponent<PlayerMover>();

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(playerRoot.transform, false);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);

            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                Object.Destroy(visualCollider);
            }
        }

        private static void EnsureWaveController()
        {
            if (Object.FindFirstObjectByType<WaveController>() != null)
            {
                return;
            }

            GameObject waveObject = new GameObject(RuntimeWaveObjectName);
            waveObject.AddComponent<EnemySpawner>();
            waveObject.AddComponent<WaveController>();
        }

        private static void EnsureSummonService()
        {
            if (Object.FindFirstObjectByType<SummonService>() != null)
            {
                return;
            }

            GameObject runtimeObject = new GameObject(RuntimeSummonServiceName);
            runtimeObject.AddComponent<SummonService>();
        }

        private static void EnsureSummonRequestHandler()
        {
            if (Object.FindFirstObjectByType<SummonRequestHandler>() != null)
            {
                return;
            }

            GameObject runtimeObject = new GameObject(RuntimeSummonRequestHandlerName);
            runtimeObject.AddComponent<SummonRequestHandler>();
        }

        private static void EnsureTowerMergeService()
        {
            if (Object.FindFirstObjectByType<TowerMergeService>() != null)
            {
                return;
            }

            GameObject runtimeObject = new GameObject(RuntimeTowerMergeServiceName);
            runtimeObject.AddComponent<TowerMergeService>();
        }

        private static void EnsureTowerSellService()
        {
            if (Object.FindFirstObjectByType<TowerSellService>() != null)
            {
                return;
            }

            GameObject runtimeObject = new GameObject(RuntimeTowerSellServiceName);
            runtimeObject.AddComponent<TowerSellService>();
        }

        private static void EnsureRunFlowController()
        {
            if (Object.FindFirstObjectByType<RunFlowController>() != null)
            {
                return;
            }

            GameObject runtimeObject = new GameObject(RuntimeRunFlowControllerName);
            runtimeObject.AddComponent<RunFlowController>();
        }

        private static void EnsureHudPresenter()
        {
            if (Object.FindFirstObjectByType<HudPresenter>() != null)
            {
                return;
            }

            GameObject runtimeObject = new GameObject(RuntimeHudName);
            runtimeObject.AddComponent<WavePanelPresenter>();
            runtimeObject.AddComponent<InteractPromptPresenter>();
            runtimeObject.AddComponent<HudPresenter>();
        }
    }
}
