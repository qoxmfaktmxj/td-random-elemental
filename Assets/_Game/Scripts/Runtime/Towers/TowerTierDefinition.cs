using TdRandomElemental.Elements;
using UnityEngine;

namespace TdRandomElemental.Towers
{
    [CreateAssetMenu(
        fileName = "TowerTierDefinition_",
        menuName = "TD Random Elemental/Data/Tower Tier Definition",
        order = 11)]
    public sealed class TowerTierDefinition : ScriptableObject
    {
        [SerializeField] private string towerId = "attack_fire_t1";
        [SerializeField] private string displayName = "Fire Attack T1";
        [SerializeField] private TowerRoleDefinition role;
        [SerializeField] private ElementDefinition element;
        [Range(1, 5)]
        [SerializeField] private int tier = 1;
        [Min(0)]
        [SerializeField] private int summonCost = 10;
        [Min(0f)]
        [SerializeField] private float damage = 5f;
        [Min(0.1f)]
        [SerializeField] private float attackRange = 4f;
        [Min(0.1f)]
        [SerializeField] private float attacksPerSecond = 1f;
        [Min(0f)]
        [SerializeField] private float projectileSpeed = 10f;
        [Min(0f)]
        [SerializeField] private float splashRadius;
        [Min(1)]
        [SerializeField] private int mergeCount = 3;
        [SerializeField] private GameObject towerPrefab;

        public string TowerId => towerId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public TowerRoleDefinition Role => role;
        public ElementDefinition Element => element;
        public int Tier => tier;
        public int SummonCost => summonCost;
        public float Damage => damage;
        public float AttackRange => attackRange;
        public float AttacksPerSecond => attacksPerSecond;
        public float ProjectileSpeed => projectileSpeed;
        public float SplashRadius => splashRadius;
        public int MergeCount => mergeCount;
        public GameObject TowerPrefab => towerPrefab;

        public void ApplyRuntimeData(
            string newTowerId,
            string newDisplayName,
            TowerRoleDefinition newRole,
            ElementDefinition newElement,
            int newTier,
            int newSummonCost,
            float newDamage,
            float newAttackRange,
            float newAttacksPerSecond,
            float newProjectileSpeed,
            float newSplashRadius,
            int newMergeCount,
            GameObject newTowerPrefab)
        {
            towerId = string.IsNullOrWhiteSpace(newTowerId) ? "runtime_tower" : newTowerId;
            displayName = string.IsNullOrWhiteSpace(newDisplayName) ? towerId : newDisplayName;
            role = newRole;
            element = newElement;
            tier = Mathf.Clamp(newTier, 1, 5);
            summonCost = Mathf.Max(0, newSummonCost);
            damage = Mathf.Max(0f, newDamage);
            attackRange = Mathf.Max(0.1f, newAttackRange);
            attacksPerSecond = Mathf.Max(0.1f, newAttacksPerSecond);
            projectileSpeed = Mathf.Max(0f, newProjectileSpeed);
            splashRadius = Mathf.Max(0f, newSplashRadius);
            mergeCount = Mathf.Max(1, newMergeCount);
            towerPrefab = newTowerPrefab;
        }
    }
}
