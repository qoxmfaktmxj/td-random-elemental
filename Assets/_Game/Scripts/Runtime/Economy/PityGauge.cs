using System;
using UnityEngine;

namespace TdRandomElemental.Economy
{
    [Serializable]
    public sealed class PityGauge
    {
        [SerializeField] private float maxValue = 100f;
        [SerializeField] private float currentValue;

        public event Action<float, float> ValueChanged;
        public event Action Filled;

        public float MaxValue => maxValue;
        public float CurrentValue => currentValue;
        public float NormalizedValue => Mathf.Approximately(maxValue, 0f) ? 0f : currentValue / maxValue;
        public bool IsFull => currentValue >= maxValue;

        public void ResetTo(float startingValue, float maxGaugeValue)
        {
            maxValue = Mathf.Max(0.01f, maxGaugeValue);
            currentValue = Mathf.Clamp(startingValue, 0f, maxValue);
            ValueChanged?.Invoke(currentValue, maxValue);
        }

        public void Add(float amount)
        {
            if (amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Pity gain must be non-negative.");
            }

            if (Mathf.Approximately(amount, 0f))
            {
                return;
            }

            float previousValue = currentValue;
            currentValue = Mathf.Clamp(currentValue + amount, 0f, maxValue);
            ValueChanged?.Invoke(currentValue, maxValue);

            if (previousValue < maxValue && Mathf.Approximately(currentValue, maxValue))
            {
                Filled?.Invoke();
            }
        }

        public bool TryConsume(float amount)
        {
            if (amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Pity consumption must be non-negative.");
            }

            if (currentValue < amount)
            {
                return false;
            }

            currentValue = Mathf.Clamp(currentValue - amount, 0f, maxValue);
            ValueChanged?.Invoke(currentValue, maxValue);
            return true;
        }

        public void Clear()
        {
            currentValue = 0f;
            ValueChanged?.Invoke(currentValue, maxValue);
        }
    }
}
