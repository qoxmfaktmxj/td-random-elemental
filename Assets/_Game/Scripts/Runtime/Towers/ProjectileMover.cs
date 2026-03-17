using TdRandomElemental.Enemies;
using TdRandomElemental.Elements;
using UnityEngine;

namespace TdRandomElemental.Towers
{
    [DisallowMultipleComponent]
    public sealed class ProjectileMover : MonoBehaviour
    {
        [SerializeField] private EnemyHealth target;
        [SerializeField] private TowerRuntime sourceTower;
        [Min(0.1f)]
        [SerializeField] private float moveSpeed = 10f;
        [Min(0.01f)]
        [SerializeField] private float hitRadius = 0.18f;
        [Min(0.1f)]
        [SerializeField] private float maxLifetime = 4f;
        [SerializeField] private Vector3 lastKnownTargetPosition;
        [SerializeField] private Color tintColor = Color.white;

        public void Initialize(
            EnemyHealth newTarget,
            TowerRuntime towerRuntime,
            Color projectileTint)
        {
            target = newTarget;
            sourceTower = towerRuntime;
            moveSpeed = Mathf.Max(0.1f, towerRuntime != null ? towerRuntime.ProjectileSpeed : 10f);
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
            float splashRadius = sourceTower != null ? sourceTower.SplashRadius : 0f;
            if (splashRadius > 0f)
            {
                ApplySplashDamage();
            }
            else if (target != null && !target.IsDead)
            {
                ElementEffectProcessor.ApplyHit(target, sourceTower, transform.position);
            }

            Destroy(gameObject);
        }

        private void ApplySplashDamage()
        {
            EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            float splashRadius = sourceTower != null ? sourceTower.SplashRadius : 0f;
            float radiusSqr = splashRadius * splashRadius;
            ElementDefinition elementDefinition = sourceTower != null ? sourceTower.ElementDefinition : null;
            bool useSingleEarthShockwave = elementDefinition != null && elementDefinition.PrimaryEffect == ElementEffectKind.Knockback;
            EnemyHealth primarySplashTarget = null;

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

                if (useSingleEarthShockwave)
                {
                    primarySplashTarget ??= enemy;
                    enemy.ApplyDamage(sourceTower != null ? sourceTower.Damage : 0f);
                    continue;
                }

                ElementEffectProcessor.ApplyHit(enemy, sourceTower, transform.position);
            }

            if (useSingleEarthShockwave && primarySplashTarget != null && sourceTower != null && elementDefinition != null)
            {
                EarthEffectHandler.Apply(
                    primarySplashTarget,
                    sourceTower.transform.position,
                    transform.position,
                    elementDefinition);
            }
        }
    }
}
