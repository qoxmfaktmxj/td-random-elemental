using TdRandomElemental.Enemies;
using UnityEngine;

namespace TdRandomElemental.Towers
{
    [DisallowMultipleComponent]
    public sealed class ProjectileMover : MonoBehaviour
    {
        [SerializeField] private EnemyHealth target;
        [Min(0.1f)]
        [SerializeField] private float moveSpeed = 10f;
        [Min(0.01f)]
        [SerializeField] private float hitRadius = 0.18f;
        [Min(0.1f)]
        [SerializeField] private float maxLifetime = 4f;
        [SerializeField] private float damage = 1f;
        [SerializeField] private float splashRadius;
        [SerializeField] private Vector3 lastKnownTargetPosition;
        [SerializeField] private Color tintColor = Color.white;

        public void Initialize(
            EnemyHealth newTarget,
            float projectileDamage,
            float projectileSpeed,
            float projectileSplashRadius,
            Color projectileTint)
        {
            target = newTarget;
            damage = Mathf.Max(0f, projectileDamage);
            moveSpeed = Mathf.Max(0.1f, projectileSpeed);
            splashRadius = Mathf.Max(0f, projectileSplashRadius);
            tintColor = projectileTint;

            lastKnownTargetPosition = GetTargetPoint();

            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = tintColor;
            }
        }

        private void Update()
        {
            maxLifetime -= Time.deltaTime;
            if (maxLifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            if (target != null && !target.IsDead)
            {
                lastKnownTargetPosition = GetTargetPoint();
            }

            Vector3 toTarget = lastKnownTargetPosition - transform.position;
            float moveDistance = moveSpeed * Time.deltaTime;
            if (toTarget.sqrMagnitude <= hitRadius * hitRadius || moveDistance >= toTarget.magnitude)
            {
                Impact();
                return;
            }

            transform.position += toTarget.normalized * moveDistance;
        }

        private Vector3 GetTargetPoint()
        {
            return target != null ? target.transform.position + Vector3.up * 0.2f : lastKnownTargetPosition;
        }

        private void Impact()
        {
            if (splashRadius > 0f)
            {
                ApplySplashDamage();
            }
            else if (target != null && !target.IsDead)
            {
                target.ApplyDamage(damage);
            }

            Destroy(gameObject);
        }

        private void ApplySplashDamage()
        {
            EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            float radiusSqr = splashRadius * splashRadius;

            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyHealth enemy = enemies[i];
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                float sqrDistance = (enemy.transform.position - transform.position).sqrMagnitude;
                if (sqrDistance > radiusSqr)
                {
                    continue;
                }

                enemy.ApplyDamage(damage);
            }
        }
    }
}
