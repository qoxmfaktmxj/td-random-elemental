using TdRandomElemental.Enemies;
using TdRandomElemental.Elements;
using UnityEngine;

namespace TdRandomElemental.Towers
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TowerRuntime))]
    [RequireComponent(typeof(TowerTargetSelector))]
    public sealed class TowerAttackController : MonoBehaviour
    {
        [SerializeField] private float cooldownRemaining;

        private TowerRuntime towerRuntime;
        private TowerTargetSelector targetSelector;

        private void Awake()
        {
            towerRuntime = GetComponent<TowerRuntime>();
            targetSelector = GetComponent<TowerTargetSelector>();
        }

        private void Update()
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);

            EnemyHealth target = targetSelector.SelectTarget();
            if (target == null)
            {
                return;
            }

            AimAt(target.transform.position);

            if (cooldownRemaining > 0f)
            {
                return;
            }

            Fire(target);
            cooldownRemaining = towerRuntime.AttackInterval;
        }

        private void Fire(EnemyHealth target)
        {
            if (target == null || target.IsDead)
            {
                return;
            }

            if (!towerRuntime.UsesProjectile)
            {
                ElementEffectProcessor.ApplyHit(target, towerRuntime, target.transform.position);
                return;
            }

            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = $"{towerRuntime.DisplayName}_Projectile";
            projectileObject.transform.position = towerRuntime.MuzzlePosition;
            projectileObject.transform.localScale = Vector3.one * 0.22f;

            Collider projectileCollider = projectileObject.GetComponent<Collider>();
            if (projectileCollider != null)
            {
                Destroy(projectileCollider);
            }

            ProjectileMover projectileMover = projectileObject.AddComponent<ProjectileMover>();
            projectileMover.Initialize(
                target,
                towerRuntime,
                towerRuntime.PrimaryColor);
        }

        private void AimAt(Vector3 worldPosition)
        {
            Transform pivot = towerRuntime.AimPivot;
            if (pivot == null)
            {
                return;
            }

            Vector3 flatDirection = worldPosition - pivot.position;
            flatDirection.y = 0f;
            if (flatDirection.sqrMagnitude <= 0.001f)
            {
                return;
            }

            pivot.rotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
        }
    }
}
