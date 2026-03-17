using UnityEngine;

namespace TdRandomElemental.Enemies
{
    public enum EnemyArchetype
    {
        Basic = 0,
        Fast = 1,
        Tank = 2,
        Boss = 3
    }

    [CreateAssetMenu(
        fileName = "EnemyDefinition_",
        menuName = "TD Random Elemental/Data/Enemy Definition",
        order = 20)]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string enemyId = "basic_enemy";
        [SerializeField] private string displayName = "Basic Enemy";
        [SerializeField] private EnemyArchetype archetype = EnemyArchetype.Basic;
        [Min(1f)]
        [SerializeField] private float maxHealth = 10f;
        [Min(0.1f)]
        [SerializeField] private float moveSpeed = 2f;
        [Min(1)]
        [SerializeField] private int coreDamage = 1;
        [Min(0)]
        [SerializeField] private int goldReward = 1;
        [SerializeField] private Vector3 modelScale = Vector3.one;
        [SerializeField] private GameObject prefab;

        public string EnemyId => enemyId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public EnemyArchetype Archetype => archetype;
        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public int CoreDamage => coreDamage;
        public int GoldReward => goldReward;
        public Vector3 ModelScale => modelScale;
        public GameObject Prefab => prefab;
    }
}
