using System;
using TdRandomElemental.Board;
using TdRandomElemental.Core;
using TdRandomElemental.Economy;
using TdRandomElemental.Towers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TdRandomElemental.Summoning
{
    [AddComponentMenu("TD Random Elemental/Tower Sell Service")]
    [DefaultExecutionOrder(-203)]
    public sealed class TowerSellService : MonoBehaviour
    {
        private const string RuntimeObjectName = "__TowerSellService";

        public static TowerSellService Instance { get; private set; }

        public event Action<TowerNode, int> TowerSold;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeInstance()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != StartupConfig.MvpArenaSceneName)
            {
                return;
            }

            if (FindFirstObjectByType<TowerSellService>() != null)
            {
                return;
            }

            GameObject runtimeObject = new GameObject(RuntimeObjectName);
            runtimeObject.AddComponent<TowerSellService>();
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

        public bool TrySellNode(TowerNode node, out int refund, out string failureReason)
        {
            refund = 0;

            if (node == null || !node.IsOccupied || node.Occupancy.OccupantObject == null)
            {
                failureReason = "Node has no tower to sell.";
                return false;
            }

            TowerRuntime towerRuntime = node.Occupancy.OccupantObject.GetComponent<TowerRuntime>();
            if (towerRuntime == null)
            {
                failureReason = "Tower component was missing.";
                return false;
            }

            RunStateService runStateService = RunStateService.Instance;
            if (runStateService == null || TowerBoardService.Instance == null)
            {
                failureReason = "Sell services were not available.";
                return false;
            }

            refund = runStateService.CalculateSellRefund(Mathf.Max(runStateService.BaseSummonCost, towerRuntime.SummonCost));
            TowerBoardService.Instance.ClearNode(node);

            if (refund > 0)
            {
                runStateService.AddGold(refund);
            }

            failureReason = string.Empty;
            TowerSold?.Invoke(node, refund);
            Debug.Log($"TowerSellService: Sold '{towerRuntime.DisplayName}' on '{node.NodeId}' for {refund}G.", this);
            return true;
        }

        [ContextMenu("Debug/Sell First Occupied Node")]
        private void DebugSellFirstOccupiedNode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("TowerSellService debug sell requires Play Mode.", this);
                return;
            }

            if (TowerBoardService.Instance == null)
            {
                return;
            }

            for (int i = 0; i < TowerBoardService.Instance.Nodes.Count; i++)
            {
                TowerNode node = TowerBoardService.Instance.Nodes[i];
                if (node == null || !node.IsOccupied)
                {
                    continue;
                }

                bool sold = TrySellNode(node, out _, out string failureReason);
                if (!sold && !string.IsNullOrEmpty(failureReason))
                {
                    Debug.LogWarning($"TowerSellService Debug: {failureReason}", this);
                }

                return;
            }

            Debug.Log("TowerSellService Debug: No occupied node found.", this);
        }
    }
}
