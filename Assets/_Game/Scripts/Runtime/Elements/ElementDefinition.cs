using System;
using UnityEngine;

namespace TdRandomElemental.Elements
{
    public enum ElementEffectKind
    {
        None = 0,
        Burn = 1,
        Slow = 2,
        Knockback = 3
    }

    [Serializable]
    public struct ElementStatusTuning
    {
        [Min(0f)]
        [SerializeField] private float damagePerTick;

        [Min(0.01f)]
        [SerializeField] private float tickInterval;

        [Min(0f)]
        [SerializeField] private float burnDuration;

        [Range(0f, 1f)]
        [SerializeField] private float slowPercent;

        [Min(0f)]
        [SerializeField] private float slowDuration;

        [Min(0f)]
        [SerializeField] private float knockbackForce;

        [Min(0f)]
        [SerializeField] private float impactRadius;

        public ElementStatusTuning(
            float damagePerTick,
            float tickInterval,
            float burnDuration,
            float slowPercent,
            float slowDuration,
            float knockbackForce,
            float impactRadius)
        {
            this.damagePerTick = Mathf.Max(0f, damagePerTick);
            this.tickInterval = Mathf.Max(0.01f, tickInterval);
            this.burnDuration = Mathf.Max(0f, burnDuration);
            this.slowPercent = Mathf.Clamp01(slowPercent);
            this.slowDuration = Mathf.Max(0f, slowDuration);
            this.knockbackForce = Mathf.Max(0f, knockbackForce);
            this.impactRadius = Mathf.Max(0f, impactRadius);
        }

        public float DamagePerTick => damagePerTick;
        public float TickInterval => tickInterval;
        public float BurnDuration => burnDuration;
        public float SlowPercent => slowPercent;
        public float SlowDuration => slowDuration;
        public float KnockbackForce => knockbackForce;
        public float ImpactRadius => impactRadius;
    }

    [CreateAssetMenu(
        fileName = "ElementDefinition_",
        menuName = "TD Random Elemental/Data/Element Definition",
        order = 0)]
    public sealed class ElementDefinition : ScriptableObject
    {
        [SerializeField] private string elementId = "fire";
        [SerializeField] private string displayName = "Fire";
        [TextArea]
        [SerializeField] private string description = "Adds a secondary elemental effect.";
        [SerializeField] private Color primaryColor = Color.red;
        [SerializeField] private Color secondaryColor = Color.yellow;
        [SerializeField] private ElementEffectKind primaryEffect = ElementEffectKind.Burn;
        [SerializeField] private ElementStatusTuning statusTuning;

        public string ElementId => elementId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public Color PrimaryColor => primaryColor;
        public Color SecondaryColor => secondaryColor;
        public ElementEffectKind PrimaryEffect => primaryEffect;
        public ElementStatusTuning StatusTuning => statusTuning;

        public void ApplyRuntimeData(
            string newElementId,
            string newDisplayName,
            string newDescription,
            Color newPrimaryColor,
            Color newSecondaryColor,
            ElementEffectKind newPrimaryEffect,
            ElementStatusTuning? newStatusTuning = null)
        {
            elementId = string.IsNullOrWhiteSpace(newElementId) ? "runtime_element" : newElementId;
            displayName = string.IsNullOrWhiteSpace(newDisplayName) ? elementId : newDisplayName;
            description = newDescription ?? string.Empty;
            primaryColor = newPrimaryColor;
            secondaryColor = newSecondaryColor;
            primaryEffect = newPrimaryEffect;
            statusTuning = newStatusTuning ?? default;
        }
    }
}
