using System;
using UnityEngine;

namespace TdRandomElemental.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        [Min(1f)]
        [SerializeField] private float fallbackMaxHealth = 10f;
        [SerializeField] private float currentHealth;

        private bool isDead;

        public event Action<EnemyHealth, float, float> HealthChanged;
        public event Action<EnemyHealth> Died;

        public EnemyDefinition Definition => definition;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => definition != null ? definition.MaxHealth : fallbackMaxHealth;
        public bool IsDead => isDead;

        private void Awake()
        {
            if (currentHealth <= 0f)
            {
                currentHealth = MaxHealth;
            }
        }

        public void Initialize(EnemyDefinition enemyDefinition)
        {
            definition = enemyDefinition;
            currentHealth = MaxHealth;
            HealthChanged?.Invoke(this, currentHealth, MaxHealth);
        }

        public void ApplyDamage(float damage)
        {
            if (damage <= 0f || isDead)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - damage);
            HealthChanged?.Invoke(this, currentHealth, MaxHealth);

            if (Mathf.Approximately(currentHealth, 0f))
            {
                isDead = true;
                Died?.Invoke(this);
            }
        }

        [ContextMenu("Debug/Apply 5 Damage")]
        private void DebugApplyDamage()
        {
            ApplyDamage(5f);
            Debug.Log($"EnemyHealth Debug: {name} HP = {currentHealth}/{MaxHealth}.", this);
        }

        [ContextMenu("Debug/Kill Enemy")]
        private void DebugKill()
        {
            ApplyDamage(MaxHealth);
        }
    }
}
