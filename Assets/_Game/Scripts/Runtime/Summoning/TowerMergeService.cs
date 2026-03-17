using System;
using System.Collections.Generic;
using TdRandomElemental.Board;
using TdRandomElemental.Core;
using TdRandomElemental.Towers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TdRandomElemental.Summoning
{
    [AddComponentMenu("TD Random Elemental/Tower Merge Service")]
    [DefaultExecutionOrder(-204)]
    public sealed class TowerMergeService : MonoBehaviour
    {
        private const string RuntimeObjectName = "__TowerMergeService";

        private readonly TowerMergeQuery mergeQuery = new TowerMergeQuery();

        public static TowerMergeService Instance { get; private set; }

        public event Action<TowerRuntime, TowerNode> TowerMerged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeInstance()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != StartupConfig.MvpArenaSceneName)
            {
                return;
            }

            if (FindFirstObjectByType<TowerMergeService>() != null)
            {
                return;
            }

            GameObject runtimeObject = new GameObject(RuntimeObjectName);
            runtimeObject.AddComponent<TowerMergeService>();
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

        private void Start()
        {
            SubscribeBoardEvents();
            RefreshMergeCandidates();
        }

        private void OnDestroy()
        {
            UnsubscribeBoardEvents();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool TryMergeNode(TowerNode node, out TowerRuntime mergedTower, out string failureReason)
        {
            mergedTower = null;

            if (node == null || !node.IsOccupied || node.Occupancy.OccupantObject == null)
            {
                failureReason = "Node has no tower to merge.";
                return false;
            }

            TowerRuntime sourceTower = node.Occupancy.OccupantObject.GetComponent<TowerRuntime>();
            if (sourceTower == null)
            {
                failureReason = "Tower component was missing.";
                return false;
            }

            if (sourceTower.IsMaxTier)
            {
                failureReason = $"{sourceTower.DisplayName} is already max tier.";
                return false;
            }

            List<TowerRuntime> mergeCandidates = mergeQuery.CollectMatchingTowers(sourceTower);
            if (mergeCandidates.Count < sourceTower.MergeCount)
            {
                failureReason = $"Need {sourceTower.MergeCount} matching towers to merge.";
                return false;
            }

            List<TowerRuntime> selectedTowers = SelectMergeTowers(sourceTower, mergeCandidates);
            TowerTierDefinition upgradeDefinition = BuildUpgradeDefinition(sourceTower);
            if (upgradeDefinition == null)
            {
                failureReason = "Failed to build upgrade definition.";
                return false;
            }

            TowerNode anchorNode = sourceTower.BoundNode != null ? sourceTower.BoundNode : node;
            TowerBoardService boardService = TowerBoardService.Instance;
            for (int i = 0; i < selectedTowers.Count; i++)
            {
                TowerRuntime tower = selectedTowers[i];
                TowerNode towerNode = tower != null ? tower.BoundNode : null;
                if (towerNode == null || boardService == null)
                {
                    continue;
                }

                boardService.ClearNode(towerNode);
            }

            mergedTower = TowerRuntime.SpawnAtNode(anchorNode, upgradeDefinition);
            if (mergedTower == null)
            {
                failureReason = "Merged tower spawn failed.";
                RefreshMergeCandidates();
                return false;
            }

            failureReason = string.Empty;
            RefreshMergeCandidates();
            TowerMerged?.Invoke(mergedTower, anchorNode);
            Debug.Log($"TowerMergeService: Merged into '{mergedTower.DisplayName}' on '{anchorNode.NodeId}'.", this);
            return true;
        }

        public void RefreshMergeCandidates()
        {
            TowerBoardService boardService = TowerBoardService.Instance;
            if (boardService == null)
            {
                return;
            }

            IReadOnlyList<TowerNode> nodes = boardService.Nodes;
            Dictionary<string, List<TowerRuntime>> groups = new Dictionary<string, List<TowerRuntime>>();

            for (int i = 0; i < nodes.Count; i++)
            {
                TowerNode node = nodes[i];
                if (node == null)
                {
                    continue;
                }

                node.SetMergeCandidate(false);

                if (!node.IsOccupied || node.Occupancy.OccupantObject == null)
                {
                    continue;
                }

                TowerRuntime towerRuntime = node.Occupancy.OccupantObject.GetComponent<TowerRuntime>();
                if (towerRuntime == null || towerRuntime.IsMaxTier)
                {
                    continue;
                }

                if (!groups.TryGetValue(towerRuntime.MergeSignature, out List<TowerRuntime> towers))
                {
                    towers = new List<TowerRuntime>();
                    groups.Add(towerRuntime.MergeSignature, towers);
                }

                towers.Add(towerRuntime);
            }

            foreach (KeyValuePair<string, List<TowerRuntime>> entry in groups)
            {
                List<TowerRuntime> towers = entry.Value;
                if (towers.Count == 0)
                {
                    continue;
                }

                int requiredCount = towers[0].MergeCount;
                if (towers.Count < requiredCount)
                {
                    continue;
                }

                for (int i = 0; i < towers.Count; i++)
                {
                    TowerNode boundNode = towers[i].BoundNode;
                    if (boundNode != null)
                    {
                        boundNode.SetMergeCandidate(true);
                    }
                }
            }
        }

        [ContextMenu("Debug/Merge First Candidate Group")]
        private void DebugMergeFirstCandidateGroup()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("TowerMergeService debug merge requires Play Mode.", this);
                return;
            }

            TowerBoardService boardService = TowerBoardService.Instance;
            if (boardService == null)
            {
                Debug.LogWarning("TowerMergeService Debug: Board service missing.", this);
                return;
            }

            IReadOnlyList<TowerNode> nodes = boardService.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                TowerNode node = nodes[i];
                if (node == null || !node.IsMergeCandidate)
                {
                    continue;
                }

                bool didMerge = TryMergeNode(node, out _, out string failureReason);
                if (!didMerge && !string.IsNullOrEmpty(failureReason))
                {
                    Debug.LogWarning($"TowerMergeService Debug: {failureReason}", this);
                }

                return;
            }

            Debug.Log("TowerMergeService Debug: No merge candidate group found.", this);
        }

        private void SubscribeBoardEvents()
        {
            if (TowerBoardService.Instance != null)
            {
                TowerBoardService.Instance.BoardChanged -= HandleBoardChanged;
                TowerBoardService.Instance.BoardChanged += HandleBoardChanged;
            }
        }

        private void UnsubscribeBoardEvents()
        {
            if (TowerBoardService.Instance != null)
            {
                TowerBoardService.Instance.BoardChanged -= HandleBoardChanged;
            }
        }

        private void HandleBoardChanged()
        {
            RefreshMergeCandidates();
        }

        private static List<TowerRuntime> SelectMergeTowers(TowerRuntime sourceTower, List<TowerRuntime> mergeCandidates)
        {
            List<TowerRuntime> selectedTowers = new List<TowerRuntime>();
            selectedTowers.Add(sourceTower);

            for (int i = 0; i < mergeCandidates.Count; i++)
            {
                TowerRuntime candidate = mergeCandidates[i];
                if (candidate == null || ReferenceEquals(candidate, sourceTower))
                {
                    continue;
                }

                selectedTowers.Add(candidate);
                if (selectedTowers.Count >= sourceTower.MergeCount)
                {
                    break;
                }
            }

            return selectedTowers;
        }

        private static TowerTierDefinition BuildUpgradeDefinition(TowerRuntime sourceTower)
        {
            if (sourceTower == null)
            {
                return null;
            }

            int nextTier = sourceTower.Tier + 1;
            float damageMultiplier = 1.8f;
            float rangeBonus = 0.35f * sourceTower.Tier;
            float attackSpeedMultiplier = 1.12f;
            float projectileSpeedBonus = 0.8f;
            float splashRadius = sourceTower.SplashRadius > 0f
                ? sourceTower.SplashRadius + 0.2f
                : 0f;

            TowerTierDefinition definition = ScriptableObject.CreateInstance<TowerTierDefinition>();
            definition.name = $"Merged_{sourceTower.RoleId}_{sourceTower.ElementId}_T{nextTier}";
            definition.ApplyRuntimeData(
                $"{sourceTower.RoleId}_{sourceTower.ElementId}_t{nextTier}",
                $"{GetDisplayLabel(sourceTower.ElementId)} {GetDisplayLabel(sourceTower.RoleId)} T{nextTier}",
                sourceTower.Definition != null ? sourceTower.Definition.Role : null,
                sourceTower.Definition != null ? sourceTower.Definition.Element : null,
                nextTier,
                Mathf.Max(1, sourceTower.SummonCost) * sourceTower.MergeCount,
                sourceTower.Damage * damageMultiplier,
                sourceTower.AttackRange + rangeBonus,
                sourceTower.AttacksPerSecond * attackSpeedMultiplier,
                sourceTower.ProjectileSpeed + projectileSpeedBonus,
                splashRadius,
                sourceTower.MergeCount,
                sourceTower.Definition != null ? sourceTower.Definition.TowerPrefab : null);
            return definition;
        }

        private static string GetDisplayLabel(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return "Runtime";
            }

            string normalized = rawValue.Replace('_', ' ');
            return char.ToUpperInvariant(normalized[0]) + normalized.Substring(1);
        }
    }
}
