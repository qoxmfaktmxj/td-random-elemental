using System;
using UnityEngine;

namespace TdRandomElemental.Economy
{
    [Serializable]
    public sealed class GoldWallet
    {
        [SerializeField] private int currentGold;

        public event Action<int> ValueChanged;

        public int CurrentGold => currentGold;

        public void ResetTo(int startingGold)
        {
            currentGold = Mathf.Max(0, startingGold);
            ValueChanged?.Invoke(currentGold);
        }

        public bool CanAfford(int amount)
        {
            return amount >= 0 && currentGold >= amount;
        }

        public bool TrySpend(int amount)
        {
            if (!CanAfford(amount))
            {
                return false;
            }

            currentGold -= amount;
            ValueChanged?.Invoke(currentGold);
            return true;
        }

        public void Add(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Gold amount must be non-negative.");
            }

            currentGold += amount;
            ValueChanged?.Invoke(currentGold);
        }
    }
}
