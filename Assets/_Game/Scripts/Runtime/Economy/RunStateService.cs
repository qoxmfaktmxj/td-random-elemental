using System;
using UnityEngine;

namespace TdRandomElemental.Economy
{
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("TD Random Elemental/Run State Service")]
    public sealed class RunStateService : MonoBehaviour
    {
        private const string RuntimeObjectName = "__RunStateService";

        [Header("Startup")]
        [Min(0)]
        [SerializeField] private int startingGold = 40;

        [Min(1)]
        [SerializeField] private int startingCoreHealth = 20;

        [Min(0)]
        [SerializeField] private int baseSummonCost = 10;

        [Range(0f, 1f)]
        [SerializeField] private float sellRefundRate = 0.6f;

        [Min(0f)]
        [SerializeField] private float startingPity;

        [Min(0.01f)]
        [SerializeField] private float maxPity = 100f;

        [Header("Runtime State")]
        [SerializeField] private GoldWallet goldWallet = new GoldWallet();
        [SerializeField] private CoreHealth coreHealth = new CoreHealth();
        [SerializeField] private PityGauge pityGauge = new PityGauge();

        public static RunStateService Instance { get; private set; }

        public event Action<int> GoldChanged;
        public event Action<int, int> CoreHealthChanged;
        public event Action RunLost;
        public event Action<float, float> PityChanged;
        public event Action PityFilled;

        public int StartingGold => startingGold;
        public int StartingCoreHealth => startingCoreHealth;
        public int BaseSummonCost => baseSummonCost;
        public float SellRefundRate => sellRefundRate;

        public GoldWallet GoldWallet => goldWallet;
        public CoreHealth CoreHealth => coreHealth;
        public PityGauge PityGauge => pityGauge;

        public bool IsRunLost => coreHealth.IsDepleted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureRuntimeInstance()
        {
            if (Instance != null)
            {
                return;
            }

            RunStateService existingInstance = FindFirstObjectByType<RunStateService>();
            if (existingInstance != null)
            {
                Instance = existingInstance;
                return;
            }

            GameObject runtimeObject = new GameObject(RuntimeObjectName);
            runtimeObject.AddComponent<RunStateService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            UnbindStateEvents();
            BindStateEvents();
            ResetForNewRun();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                UnbindStateEvents();
                Instance = null;
            }
        }

        public void ResetForNewRun()
        {
            goldWallet.ResetTo(startingGold);
            coreHealth.ResetTo(startingCoreHealth);
            pityGauge.ResetTo(startingPity, maxPity);
        }

        public bool TrySpendGold(int amount)
        {
            return goldWallet.TrySpend(amount);
        }

        public void AddGold(int amount)
        {
            goldWallet.Add(amount);
        }

        public void ApplyCoreDamage(int damage)
        {
            coreHealth.ApplyDamage(damage);
        }

        public void AddPity(float amount)
        {
            pityGauge.Add(amount);
        }

        public bool TryConsumePity(float amount)
        {
            return pityGauge.TryConsume(amount);
        }

        public void ClearPity()
        {
            pityGauge.Clear();
        }

        public int CalculateSellRefund(int originalCost)
        {
            if (originalCost <= 0)
            {
                return 0;
            }

            return Mathf.FloorToInt(originalCost * sellRefundRate);
        }

        [ContextMenu("Debug/Add 10 Gold")]
        private void DebugAddGold()
        {
            AddGold(10);
            Debug.Log($"RunStateService Debug: Gold is now {goldWallet.CurrentGold}.", this);
        }

        [ContextMenu("Debug/Spend Base Summon Cost")]
        private void DebugSpendBaseSummonCost()
        {
            bool didSpend = TrySpendGold(baseSummonCost);
            Debug.Log($"RunStateService Debug: Spend {baseSummonCost} result = {didSpend}, gold = {goldWallet.CurrentGold}.", this);
        }

        [ContextMenu("Debug/Damage Core By 1")]
        private void DebugDamageCore()
        {
            ApplyCoreDamage(1);
            Debug.Log($"RunStateService Debug: Core HP = {coreHealth.CurrentHealth}/{coreHealth.MaxHealth}.", this);
        }

        [ContextMenu("Debug/Add 25 Pity")]
        private void DebugAddPity()
        {
            AddPity(25f);
            Debug.Log($"RunStateService Debug: Pity = {pityGauge.CurrentValue}/{pityGauge.MaxValue}.", this);
        }

        [ContextMenu("Debug/Reset Run State")]
        private void DebugResetRunState()
        {
            ResetForNewRun();
            Debug.Log("RunStateService Debug: Run state reset to startup values.", this);
        }

        private void OnValidate()
        {
            startingGold = Mathf.Max(0, startingGold);
            startingCoreHealth = Mathf.Max(1, startingCoreHealth);
            baseSummonCost = Mathf.Max(0, baseSummonCost);
            sellRefundRate = Mathf.Clamp01(sellRefundRate);
            startingPity = Mathf.Max(0f, startingPity);
            maxPity = Mathf.Max(0.01f, maxPity);
            if (startingPity > maxPity)
            {
                startingPity = maxPity;
            }
        }

        private void BindStateEvents()
        {
            goldWallet.ValueChanged += HandleGoldChanged;
            coreHealth.ValueChanged += HandleCoreHealthChanged;
            coreHealth.Depleted += HandleCoreDepleted;
            pityGauge.ValueChanged += HandlePityChanged;
            pityGauge.Filled += HandlePityFilled;
        }

        private void UnbindStateEvents()
        {
            goldWallet.ValueChanged -= HandleGoldChanged;
            coreHealth.ValueChanged -= HandleCoreHealthChanged;
            coreHealth.Depleted -= HandleCoreDepleted;
            pityGauge.ValueChanged -= HandlePityChanged;
            pityGauge.Filled -= HandlePityFilled;
        }

        private void HandleGoldChanged(int currentGold)
        {
            GoldChanged?.Invoke(currentGold);
        }

        private void HandleCoreHealthChanged(int currentHealth, int maxHealthValue)
        {
            CoreHealthChanged?.Invoke(currentHealth, maxHealthValue);
        }

        private void HandleCoreDepleted()
        {
            RunLost?.Invoke();
        }

        private void HandlePityChanged(float currentValue, float maxValue)
        {
            PityChanged?.Invoke(currentValue, maxValue);
        }

        private void HandlePityFilled()
        {
            PityFilled?.Invoke();
        }
    }
}
