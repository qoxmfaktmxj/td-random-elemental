using TdRandomElemental.Enemies;
using UnityEngine;

namespace TdRandomElemental.Elements
{
    public static class EarthEffectHandler
    {
        public static void Apply(
            EnemyHealth primaryTarget,
            Vector3 sourcePosition,
            Vector3 impactPoint,
            ElementDefinition elementDefinition)
        {
            if (primaryTarget == null || elementDefinition == null)
            {
                return;
            }

            ElementStatusTuning tuning = elementDefinition.StatusTuning;
            float impactRadius = Mathf.Max(0f, tuning.ImpactRadius);

            if (impactRadius <= 0f)
            {
                ApplyToSingleTarget(primaryTarget, sourcePosition, tuning, elementDefinition.PrimaryColor);
                return;
            }

            EnemyHealth[] enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            float radiusSqr = impactRadius * impactRadius;

            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyHealth target = enemies[i];
                if (target == null || target.IsDead)
                {
                    continue;
                }

                if ((target.transform.position - impactPoint).sqrMagnitude > radiusSqr)
                {
                    continue;
                }

                ApplyToSingleTarget(target, sourcePosition, tuning, elementDefinition.PrimaryColor);
            }
        }

        private static void ApplyToSingleTarget(
            EnemyHealth target,
            Vector3 sourcePosition,
            ElementStatusTuning tuning,
            Color tintColor)
        {
            EnemyStatusController controller = target.GetComponent<EnemyStatusController>();
            if (controller == null)
            {
                controller = target.gameObject.AddComponent<EnemyStatusController>();
            }

            Vector3 direction = target.transform.position - sourcePosition;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = Vector3.forward;
            }

            controller.ApplyImpact(direction.normalized, tuning, tintColor);
        }
    }
}
