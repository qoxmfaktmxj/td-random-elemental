using System;
using TdRandomElemental.Elements;
using UnityEngine;

namespace TdRandomElemental.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(EnemyMover))]
    public sealed class EnemyStatusController : MonoBehaviour
    {
        [Header("Burn")]
        [SerializeField] private float burnRemaining;
        [SerializeField] private float burnTickInterval = 0.5f;
        [SerializeField] private float burnTickTimer;
        [SerializeField] private float burnDamagePerTick;

        [Header("Slow")]
        [SerializeField] private float slowRemaining;
        [SerializeField] private float slowMultiplier = 1f;

        [Header("Impact")]
        [SerializeField] private float impactFlashRemaining;

        [Header("Visual")]
        [SerializeField] private Color activeTint = Color.white;

        private EnemyHealth enemyHealth;
        private EnemyMover enemyMover;
        private Renderer[] cachedRenderers = Array.Empty<Renderer>();
        private Color[] baseColors = Array.Empty<Color>();

        private void Awake()
        {
            enemyHealth = GetComponent<EnemyHealth>();
            enemyMover = GetComponent<EnemyMover>();
            cachedRenderers = GetComponentsInChildren<Renderer>(true);

            baseColors = new Color[cachedRenderers.Length];
            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                baseColors[i] = cachedRenderers[i] != null ? cachedRenderers[i].material.color : Color.white;
            }
        }

        private void OnDisable()
        {
            RestoreBaseColors();

            if (enemyMover != null)
            {
                enemyMover.SetSpeedMultiplier(1f);
            }
        }

        private void Update()
        {
            if (enemyHealth == null || enemyHealth.IsDead)
            {
                return;
            }

            UpdateBurn();
            UpdateSlow();
            UpdateImpact();
            RefreshTint();
        }

        public void ApplyBurn(ElementStatusTuning tuning, Color tintColor)
        {
            if (tuning.DamagePerTick <= 0f)
            {
                return;
            }

            burnDamagePerTick = Mathf.Max(burnDamagePerTick, tuning.DamagePerTick);
            burnTickInterval = Mathf.Max(0.05f, tuning.TickInterval);
            burnRemaining = Mathf.Max(burnRemaining, Mathf.Max(0.5f, tuning.BurnDuration));
            burnTickTimer = Mathf.Min(burnTickTimer <= 0f ? burnTickInterval : burnTickTimer, burnTickInterval);
            activeTint = tintColor;
        }

        public void ApplySlow(ElementStatusTuning tuning, Color tintColor)
        {
            if (tuning.SlowPercent <= 0f || tuning.SlowDuration <= 0f)
            {
                return;
            }

            float requestedMultiplier = 1f - Mathf.Clamp01(tuning.SlowPercent);
            slowMultiplier = Mathf.Min(slowMultiplier, requestedMultiplier);
            slowRemaining = Mathf.Max(slowRemaining, tuning.SlowDuration);
            activeTint = tintColor;

            if (enemyMover != null)
            {
                enemyMover.SetSpeedMultiplier(slowMultiplier);
            }
        }

        public void ApplyImpact(Vector3 direction, ElementStatusTuning tuning, Color tintColor)
        {
            if (enemyMover != null && direction.sqrMagnitude > 0.001f && tuning.KnockbackForce > 0f)
            {
                enemyMover.ApplyImpulse(direction.normalized * tuning.KnockbackForce);
            }

            impactFlashRemaining = Mathf.Max(impactFlashRemaining, 0.18f);
            activeTint = tintColor;
        }

        private void UpdateBurn()
        {
            if (burnRemaining <= 0f)
            {
                return;
            }

            burnRemaining = Mathf.Max(0f, burnRemaining - Time.deltaTime);
            burnTickTimer -= Time.deltaTime;
            if (burnTickTimer > 0f)
            {
                return;
            }

            burnTickTimer = burnTickInterval;
            enemyHealth.ApplyDamage(burnDamagePerTick);
        }

        private void UpdateSlow()
        {
            if (slowRemaining <= 0f)
            {
                if (slowMultiplier < 1f)
                {
                    slowMultiplier = 1f;
                    if (enemyMover != null)
                    {
                        enemyMover.SetSpeedMultiplier(1f);
                    }
                }

                return;
            }

            slowRemaining = Mathf.Max(0f, slowRemaining - Time.deltaTime);
            if (enemyMover != null)
            {
                enemyMover.SetSpeedMultiplier(slowMultiplier);
            }
        }

        private void UpdateImpact()
        {
            if (impactFlashRemaining <= 0f)
            {
                return;
            }

            impactFlashRemaining = Mathf.Max(0f, impactFlashRemaining - Time.deltaTime);
        }

        private void RefreshTint()
        {
            bool hasActiveStatus = impactFlashRemaining > 0f || burnRemaining > 0f || slowRemaining > 0f;
            if (!hasActiveStatus)
            {
                RestoreBaseColors();
                return;
            }

            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                Renderer renderer = cachedRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.material.color = Color.Lerp(baseColors[i], activeTint, 0.7f);
            }
        }

        private void RestoreBaseColors()
        {
            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                Renderer renderer = cachedRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.material.color = baseColors[i];
            }
        }
    }
}
