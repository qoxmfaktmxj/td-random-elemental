using System;
using UnityEngine;

namespace TdRandomElemental.Economy
{
    [Serializable]
    public sealed class CoreHealth
    {
        [SerializeField] private int maxHealth = 1;
        [SerializeField] private int currentHealth = 1;

        public event Action<int, int> ValueChanged;
        public event Action Depleted;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public bool IsDepleted => currentHealth <= 0;

        public void ResetTo(int startingMaxHealth)
        {
            maxHealth = Mathf.Max(1, startingMaxHealth);
            currentHealth = maxHealth;
            ValueChanged?.Invoke(currentHealth, maxHealth);
        }

        public void ApplyDamage(int damage)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "Damage must be non-negative.");
            }

            if (damage == 0 || IsDepleted)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - damage);
            ValueChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth == 0)
            {
                Depleted?.Invoke();
            }
        }

        public void RestoreToFull()
        {
            currentHealth = maxHealth;
            ValueChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
