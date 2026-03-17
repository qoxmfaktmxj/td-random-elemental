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

            PrimitiveType primitiveType = PrimitiveType.Sphere;
            Color tintColor = new Color(0.8f, 0.82f, 0.88f, 1f);

            if (definition != null)
            {
                switch (definition.Archetype)
                {
                    case EnemyArchetype.Fast:
                        primitiveType = PrimitiveType.Capsule;
                        tintColor = new Color(0.45f, 0.86f, 1f, 1f);
                        break;

                    case EnemyArchetype.Tank:
                        primitiveType = PrimitiveType.Cube;
                        tintColor = new Color(0.76f, 0.63f, 0.42f, 1f);
                        break;

                    case EnemyArchetype.Boss:
                        primitiveType = PrimitiveType.Capsule;
                        tintColor = new Color(1f, 0.36f, 0.28f, 1f);
                        break;
                }
            }

            GameObject enemyObject = GameObject.CreatePrimitive(primitiveType);
            enemyObject.name = definition != null
                ? $"Enemy_{definition.DisplayName}"
                : "Enemy_Runtime";

            Renderer renderer = enemyObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = tintColor;
            }

            return enemyObject;
        }
    }
}
