using System;
using TdRandomElemental.Enemies;
using UnityEngine;

namespace TdRandomElemental.Waves
{
    [DisallowMultipleComponent]
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private PathLane pathLane;

        public event Action<EnemyHealth, EnemyMover> EnemySpawned;

        private void Awake()
        {
            if (pathLane == null)
            {
                pathLane = FindFirstObjectByType<PathLane>();
            }
        }

        public EnemyHealth SpawnEnemy(EnemyDefinition definition)
        {
            if (pathLane == null)
            {
                Debug.LogError("EnemySpawner: No PathLane found for enemy spawning.", this);
                return null;
            }

            GameObject enemyObject = CreateEnemyObject(definition);
            enemyObject.transform.position = pathLane.GetSpawnPosition() + new Vector3(0f, 0.5f, 0f);

            EnemyHealth enemyHealth = enemyObject.GetComponent<EnemyHealth>();
            if (enemyHealth == null)
            {
                enemyHealth = enemyObject.AddComponent<EnemyHealth>();
            }

            EnemyMover enemyMover = enemyObject.GetComponent<EnemyMover>();
            if (enemyMover == null)
            {
                enemyMover = enemyObject.AddComponent<EnemyMover>();
            }

            if (enemyObject.GetComponent<EnemyRewardOnDeath>() == null)
            {
                enemyObject.AddComponent<EnemyRewardOnDeath>();
            }

            enemyHealth.Initialize(definition);
            enemyMover.Initialize(pathLane, definition);

            EnemySpawned?.Invoke(enemyHealth, enemyMover);
            return enemyHealth;
        }

        private static GameObject CreateEnemyObject(EnemyDefinition definition)
        {
            if (definition != null && definition.Prefab != null)
            {
                GameObject instance = Instantiate(definition.Prefab);
                instance.name = $"Enemy_{definition.DisplayName}";
                return instance;
            }

            GameObject enemyObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            enemyObject.name = definition != null
                ? $"Enemy_{definition.DisplayName}"
                : "Enemy_Runtime";
            return enemyObject;
        }
    }
}
