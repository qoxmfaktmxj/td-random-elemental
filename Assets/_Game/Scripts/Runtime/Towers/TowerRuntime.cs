using System;
using TdRandomElemental.Board;
using TdRandomElemental.Elements;
using TdRandomElemental.UI;
using UnityEngine;

namespace TdRandomElemental.Towers
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TowerTargetSelector))]
    [RequireComponent(typeof(TowerAttackController))]
    [RequireComponent(typeof(TowerWorldIndicator))]
    public sealed class TowerRuntime : MonoBehaviour
    {
        private const string RuntimeVisualRootName = "RuntimeVisual";

        [Header("Definition")]
        [SerializeField] private TowerTierDefinition definition;
        [SerializeField] private TowerNode boundNode;

        [Header("Fallback")]
        [SerializeField] private string fallbackTowerId = "debug_attack_fire_t1";
        [SerializeField] private string fallbackDisplayName = "Debug Attack Tower";
        [SerializeField] private TowerTargetingMode fallbackTargetingMode = TowerTargetingMode.First;
        [Min(0.1f)]
        [SerializeField] private float fallbackDamage = 4f;
        [Min(0.1f)]
        [SerializeField] private float fallbackAttackRange = 5.5f;
        [Min(0.1f)]
        [SerializeField] private float fallbackAttacksPerSecond = 1.2f;
        [Min(0.1f)]
        [SerializeField] private float fallbackProjectileSpeed = 12f;
        [SerializeField] private Color fallbackPrimaryColor = new Color(1f, 0.45f, 0.15f, 1f);
        [SerializeField] private bool fallbackUsesProjectile = true;

        [Header("Presentation")]
        [SerializeField] private Transform aimPivot;
        [SerializeField] private Transform muzzleTransform;
        [SerializeField] private Renderer[] tintedRenderers = Array.Empty<Renderer>();

        private GameObject runtimeVisualRoot;

        public TowerTierDefinition Definition => definition;
        public TowerNode BoundNode => boundNode;
        public string TowerId => definition != null ? definition.TowerId : fallbackTowerId;
        public string DisplayName => definition != null ? definition.DisplayName : fallbackDisplayName;
        public string RoleId => definition != null && definition.Role != null ? definition.Role.RoleId : "debug_role";
        public string ElementId => definition != null && definition.Element != null ? definition.Element.ElementId : "debug_element";
        public ElementDefinition ElementDefinition => definition != null ? definition.Element : null;
        public string MergeSignature => $"{RoleId}:{ElementId}:{Tier}";
        public float Damage => definition != null ? definition.Damage : fallbackDamage;
        public float AttackRange => definition != null ? definition.AttackRange : fallbackAttackRange;
        public float AttacksPerSecond => definition != null ? definition.AttacksPerSecond : fallbackAttacksPerSecond;
        public float AttackInterval => 1f / Mathf.Max(0.01f, AttacksPerSecond);
        public float ProjectileSpeed => definition != null ? definition.ProjectileSpeed : fallbackProjectileSpeed;
        public float SplashRadius => definition != null ? definition.SplashRadius : 0f;
        public int Tier => definition != null ? definition.Tier : 1;
        public int MergeCount => definition != null ? definition.MergeCount : 3;
        public int SummonCost => definition != null ? definition.SummonCost : 0;
        public bool IsMaxTier => Tier >= 5;
        public TowerTargetingMode TargetingMode => definition != null && definition.Role != null
            ? definition.Role.DefaultTargetingMode
            : fallbackTargetingMode;
        public bool UsesProjectile => definition != null && definition.Role != null
            ? definition.Role.UsesProjectile
            : fallbackUsesProjectile;
        public Color PrimaryColor => definition != null && definition.Element != null
            ? definition.Element.PrimaryColor
            : fallbackPrimaryColor;
        public Transform AimPivot => aimPivot != null ? aimPivot : transform;
        public Vector3 MuzzlePosition => muzzleTransform != null ? muzzleTransform.position : transform.position + Vector3.up * 1.1f;

        public static TowerRuntime SpawnAtNode(TowerNode node, TowerTierDefinition towerDefinition = null)
        {
            if (node == null)
            {
                return null;
            }

            TowerBoardService boardService = TowerBoardService.Instance;
            if (boardService != null && !boardService.IsNodeAvailableForOccupancy(node))
            {
                Debug.LogWarning($"TowerRuntime: Node '{node.NodeId}' is not available for tower placement.");
                return null;
            }

            GameObject towerObject = new GameObject(towerDefinition != null ? towerDefinition.DisplayName : "DebugAttackTower");
            TowerRuntime towerRuntime = towerObject.AddComponent<TowerRuntime>();
            towerRuntime.Initialize(node, towerDefinition);

            bool occupied = boardService != null
                ? boardService.TryOccupyNode(node, towerRuntime.TowerId, towerObject)
                : SetNodeOccupiedFallback(node, towerRuntime);

            if (!occupied)
            {
                Destroy(towerObject);
                return null;
            }

            return towerRuntime;
        }

        public static TowerRuntime SpawnDebugTower(TowerNode node, TowerTierDefinition towerDefinition = null)
        {
            return SpawnAtNode(node, towerDefinition);
        }

        private static bool SetNodeOccupiedFallback(TowerNode node, TowerRuntime towerRuntime)
        {
            if (node == null || towerRuntime == null || node.IsDisabled || node.IsOccupied)
            {
                return false;
            }

            node.SetOccupied(towerRuntime.TowerId, towerRuntime.gameObject);
            return true;
        }

        private void OnDestroy()
        {
            if (boundNode == null)
            {
                return;
            }

            if (boundNode.Occupancy.OccupantObject == gameObject)
            {
                boundNode.ClearOccupancy();
            }
        }

        public void Initialize(TowerNode node, TowerTierDefinition towerDefinition = null)
        {
            boundNode = node;
            definition = towerDefinition;

            if (boundNode != null)
            {
                transform.SetParent(boundNode.transform, false);
                transform.localPosition = new Vector3(0f, 0.7f, 0f);
                transform.localRotation = Quaternion.identity;
            }

            EnsurePresentation();
            RefreshPresentation();
        }

        private void EnsurePresentation()
        {
            if (definition != null && definition.TowerPrefab != null)
            {
                GameObject instance = Instantiate(definition.TowerPrefab, transform);
                instance.name = definition.TowerPrefab.name;
                runtimeVisualRoot = instance;
                CachePresentationReferences();
                return;
            }

            if (runtimeVisualRoot != null)
            {
                CachePresentationReferences();
                ApplyTierPresentation();
                return;
            }

            runtimeVisualRoot = new GameObject(RuntimeVisualRootName);
            runtimeVisualRoot.transform.SetParent(transform, false);
            runtimeVisualRoot.transform.localPosition = Vector3.zero;
            runtimeVisualRoot.transform.localRotation = Quaternion.identity;

            GameObject baseVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseVisual.name = "Base";
            baseVisual.transform.SetParent(runtimeVisualRoot.transform, false);
            baseVisual.transform.localPosition = Vector3.zero;
            baseVisual.transform.localScale = new Vector3(0.8f, 0.2f, 0.8f);

            GameObject coreVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coreVisual.name = "Core";
            coreVisual.transform.SetParent(runtimeVisualRoot.transform, false);
            coreVisual.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            coreVisual.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);

            GameObject barrelVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrelVisual.name = "Barrel";
            barrelVisual.transform.SetParent(runtimeVisualRoot.transform, false);
            barrelVisual.transform.localPosition = new Vector3(0f, 0.45f, 0.45f);
            barrelVisual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            barrelVisual.transform.localScale = new Vector3(0.12f, 0.38f, 0.12f);

            GameObject muzzleObject = new GameObject("Muzzle");
            muzzleObject.transform.SetParent(runtimeVisualRoot.transform, false);
            muzzleObject.transform.localPosition = new Vector3(0f, 0.45f, 0.85f);

            aimPivot = runtimeVisualRoot.transform;
            muzzleTransform = muzzleObject.transform;
            CachePresentationReferences();
            ApplyTierPresentation();
        }

        private void CachePresentationReferences()
        {
            if (aimPivot == null && runtimeVisualRoot != null)
            {
                aimPivot = runtimeVisualRoot.transform;
            }

            if (muzzleTransform == null)
            {
                Transform muzzle = transform.Find($"{RuntimeVisualRootName}/Muzzle");
                if (muzzle != null)
                {
                    muzzleTransform = muzzle;
                }
            }

            tintedRenderers = GetComponentsInChildren<Renderer>(true);
        }

        private void RefreshPresentation()
        {
            CachePresentationReferences();
            Color tintColor = PrimaryColor;
            for (int i = 0; i < tintedRenderers.Length; i++)
            {
                Renderer renderer = tintedRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.material.color = tintColor;
            }

            ApplyTierPresentation();
        }

        private void ApplyTierPresentation()
        {
            if (runtimeVisualRoot == null)
            {
                return;
            }

            float tierScaleMultiplier = 1f + (Mathf.Max(1, Tier) - 1) * 0.18f;
            runtimeVisualRoot.transform.localScale = Vector3.one * tierScaleMultiplier;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(PrimaryColor.r, PrimaryColor.g, PrimaryColor.b, 0.8f);
            Gizmos.DrawWireSphere(transform.position, AttackRange);
        }
    }
}
