using TdRandomElemental.Board;
using TdRandomElemental.Core;
using TdRandomElemental.Towers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TdRandomElemental.Summoning
{
    [AddComponentMenu("TD Random Elemental/Summon Request Handler")]
    [DefaultExecutionOrder(-205)]
    public sealed class SummonRequestHandler : MonoBehaviour
    {
        private const string RuntimeObjectName = "__SummonRequestHandler";

        public static SummonRequestHandler Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeInstance()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != StartupConfig.MvpArenaSceneName)
            {
                return;
            }

            if (FindFirstObjectByType<SummonRequestHandler>() != null)
            {
                return;
            }

            GameObject runtimeObject = new GameObject(RuntimeObjectName);
            runtimeObject.AddComponent<SummonRequestHandler>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool HandleNodeInteraction(TowerNode node, GameObject interactor)
        {
            if (node == null)
            {
                return false;
            }

            if (node.IsOccupied)
            {
                TowerRuntime towerRuntime = node.Occupancy.OccupantObject != null
                    ? node.Occupancy.OccupantObject.GetComponent<TowerRuntime>()
                    : null;

                string mergeFailureReason = string.Empty;
                if (TowerMergeService.Instance != null && TowerMergeService.Instance.TryMergeNode(node, out TowerRuntime mergedTower, out mergeFailureReason))
                {
                    Debug.Log($"SummonRequestHandler: Merged into '{mergedTower.DisplayName}' on '{node.NodeId}'.", node);
                    return true;
                }

                string displayName = towerRuntime != null ? towerRuntime.DisplayName : node.Occupancy.OccupantId;
                if (!string.IsNullOrEmpty(mergeFailureReason))
                {
                    Debug.Log($"SummonRequestHandler: Merge unavailable for '{displayName}' on '{node.NodeId}'. reason={mergeFailureReason}", node);
                }
                else
                {
                    Debug.Log($"SummonRequestHandler: Node '{node.NodeId}' already has '{displayName}'.", node);
                }

                return true;
            }

            if (SummonService.Instance == null)
            {
                Debug.LogWarning("SummonRequestHandler: SummonService instance was missing.", this);
                return true;
            }

            bool didSummon = SummonService.Instance.TrySummonToNode(node, out _, out string failureReason);
            if (!didSummon && !string.IsNullOrEmpty(failureReason))
            {
                Debug.LogWarning($"SummonRequestHandler: {failureReason}", node);
            }

            return true;
        }
    }
}
