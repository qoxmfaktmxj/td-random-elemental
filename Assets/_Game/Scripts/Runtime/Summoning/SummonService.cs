using System;
using System.Collections.Generic;
using TdRandomElemental.Board;
using TdRandomElemental.Core;
using TdRandomElemental.Economy;
using TdRandomElemental.Elements;
using TdRandomElemental.Towers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TdRandomElemental.Summoning
{
    [AddComponentMenu("TD Random Elemental/Summon Service")]
    [DefaultExecutionOrder(-210)]
    public sealed class SummonService : MonoBehaviour
    {
        private const string RuntimeObjectName = "__SummonService";

        [Header("Pool")]
        [SerializeField] private List<TowerTierDefinition> configuredTowerPool = new List<TowerTierDefinition>();
        [SerializeField] private bool useRuntimeFallbackPool = true;

        [Header("Runtime")]
        [SerializeField] private SummonPool summonPool = new SummonPool();

        private readonly TowerFactory towerFactory = new TowerFactory();

        public static SummonService Instance { get; private set; }

        public event Action<TowerRuntime, TowerNode, int> TowerSummoned;
        public event Action<TowerNode, string> SummonFailed;

        public SummonPool Pool => summonPool;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeInstance()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != StartupConfig.MvpArenaSceneName)
            {
                return;
            }

            if (FindFirstObjectByType<SummonService>() != null)
            {
                return;
            }

            GameObject runtimeObject = new GameObject(RuntimeObjectName);
            runtimeObject.AddComponent<SummonService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            RebuildPool();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool TrySummonToNode(TowerNode node, out TowerRuntime towerRuntime, out string failureReason)
        {
            towerRuntime = null;

            if (node == null)
            {
                failureReason = "Node was missing.";
                SummonFailed?.Invoke(node, failureReason);
                return false;
            }

            if (TowerBoardService.Instance != null && !TowerBoardService.Instance.IsNodeAvailableForOccupancy(node))
            {
                failureReason = $"Node '{node.NodeId}' is not empty.";
                SummonFailed?.Invoke(node, failureReason);
                return false;
            }

            TowerTierDefinition definition = summonPool.RollRandomTower();
            if (definition == null)
            {
                failureReason = "Summon pool is empty.";
                SummonFailed?.Invoke(node, failureReason);
                return false;
            }

            RunStateService runStateService = RunStateService.Instance;
            if (runStateService == null)
            {
                failureReason = "Run state service was missing.";
                SummonFailed?.Invoke(node, failureReason);
                return false;
            }

            int summonCost = definition.SummonCost > 0 ? definition.SummonCost : runStateService.BaseSummonCost;
            if (!runStateService.TrySpendGold(summonCost))
            {
                failureReason = $"Not enough gold for {definition.DisplayName} ({summonCost}G).";
                SummonFailed?.Invoke(node, failureReason);
                return false;
            }

            towerRuntime = towerFactory.CreateTower(node, definition);
            if (towerRuntime == null)
            {
                runStateService.AddGold(summonCost);
                failureReason = $"Failed to create {definition.DisplayName}.";
                SummonFailed?.Invoke(node, failureReason);
                return false;
            }

            failureReason = string.Empty;
            TowerSummoned?.Invoke(towerRuntime, node, summonCost);
            Debug.Log($"SummonService: Summoned '{definition.DisplayName}' on '{node.NodeId}' for {summonCost}G.", this);
            return true;
        }

        [ContextMenu("Debug/Summon On First Empty Node")]
        private void DebugSummonFirstEmptyNode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("SummonService debug summon requires Play Mode.", this);
                return;
            }

            TowerNode availableNode = TowerBoardService.Instance != null
                ? TowerBoardService.Instance.GetFirstAvailableNode()
                : null;

            if (availableNode == null)
            {
                Debug.LogWarning("SummonService Debug: No empty node is available.", this);
                return;
            }

            TrySummonToNode(availableNode, out _, out string failureReason);
            if (!string.IsNullOrEmpty(failureReason))
            {
                Debug.LogWarning($"SummonService Debug: {failureReason}", this);
            }
        }

        private void RebuildPool()
        {
            if (configuredTowerPool.Count > 0)
            {
                List<TowerTierDefinition> summonableDefinitions = new List<TowerTierDefinition>();
                for (int i = 0; i < configuredTowerPool.Count; i++)
                {
                    TowerTierDefinition definition = configuredTowerPool[i];
                    if (definition != null && definition.Tier == 1)
                    {
                        summonableDefinitions.Add(definition);
                    }
                }

                summonPool.SetDefinitions(summonableDefinitions);
                return;
            }

            if (!useRuntimeFallbackPool)
            {
                summonPool.SetDefinitions(Array.Empty<TowerTierDefinition>());
                return;
            }

            summonPool.SetDefinitions(BuildRuntimeFallbackPool());
        }

        private static IReadOnlyList<TowerTierDefinition> BuildRuntimeFallbackPool()
        {
            ElementDefinition fire = CreateRuntimeElement(
                "fire",
                "Fire",
                new Color(1f, 0.42f, 0.16f, 1f),
                new Color(1f, 0.78f, 0.2f, 1f),
                ElementEffectKind.Burn,
                new ElementStatusTuning(0.85f, 0.35f, 1.75f, 0f, 0f, 0f, 0f));

            ElementDefinition water = CreateRuntimeElement(
                "water",
                "Water",
                new Color(0.2f, 0.75f, 1f, 1f),
                new Color(0.7f, 0.95f, 1f, 1f),
                ElementEffectKind.Slow,
                new ElementStatusTuning(0f, 0.35f, 0f, 0.4f, 1.6f, 0f, 0f));

            ElementDefinition earth = CreateRuntimeElement(
                "earth",
                "Earth",
                new Color(0.67f, 0.54f, 0.28f, 1f),
                new Color(0.88f, 0.78f, 0.46f, 1f),
                ElementEffectKind.Knockback,
                new ElementStatusTuning(0f, 0.35f, 0f, 0f, 0f, 6f, 1.35f));

            TowerRoleDefinition attack = CreateRuntimeRole(
                "attack",
                "Attack",
                TowerRoleKind.Attack,
                TowerTargetingMode.First,
                true);

            TowerRoleDefinition control = CreateRuntimeRole(
                "control",
                "Control",
                TowerRoleKind.Control,
                TowerTargetingMode.Closest,
                true);

            TowerRoleDefinition impact = CreateRuntimeRole(
                "impact",
                "Impact",
                TowerRoleKind.Impact,
                TowerTargetingMode.Strongest,
                true);

            return new[]
            {
                CreateRuntimeTower("attack_fire_t1", "Fire Attack T1", attack, fire, 1, 10, 4.2f, 5.5f, 1.2f, 12f, 0f),
                CreateRuntimeTower("control_water_t1", "Water Control T1", control, water, 1, 10, 2.8f, 6f, 1.55f, 11f, 0f),
                CreateRuntimeTower("impact_earth_t1", "Earth Impact T1", impact, earth, 1, 10, 6.5f, 4.5f, 0.75f, 8.5f, 1.15f)
            };
        }

        private static ElementDefinition CreateRuntimeElement(
            string elementId,
            string displayName,
            Color primaryColor,
            Color secondaryColor,
            ElementEffectKind primaryEffect,
            ElementStatusTuning statusTuning)
        {
            ElementDefinition definition = ScriptableObject.CreateInstance<ElementDefinition>();
            definition.name = $"RuntimeElement_{displayName}";
            definition.ApplyRuntimeData(
                elementId,
                displayName,
                $"{displayName} runtime fallback.",
                primaryColor,
                secondaryColor,
                primaryEffect,
                statusTuning);
            return definition;
        }

        private static TowerRoleDefinition CreateRuntimeRole(
            string roleId,
            string displayName,
            TowerRoleKind roleKind,
            TowerTargetingMode targetingMode,
            bool usesProjectile)
        {
            TowerRoleDefinition definition = ScriptableObject.CreateInstance<TowerRoleDefinition>();
            definition.name = $"RuntimeRole_{displayName}";
            definition.ApplyRuntimeData(roleId, displayName, $"{displayName} runtime fallback.", roleKind, targetingMode, usesProjectile);
            return definition;
        }

        private static TowerTierDefinition CreateRuntimeTower(
            string towerId,
            string displayName,
            TowerRoleDefinition role,
            ElementDefinition element,
            int tier,
            int summonCost,
            float damage,
            float range,
            float attacksPerSecond,
            float projectileSpeed,
            float splashRadius)
        {
            TowerTierDefinition definition = ScriptableObject.CreateInstance<TowerTierDefinition>();
            definition.name = $"RuntimeTower_{displayName}";
            definition.ApplyRuntimeData(
                towerId,
                displayName,
                role,
                element,
                tier,
                summonCost,
                damage,
                range,
                attacksPerSecond,
                projectileSpeed,
                splashRadius,
                3,
                null);
            return definition;
        }
    }
}
