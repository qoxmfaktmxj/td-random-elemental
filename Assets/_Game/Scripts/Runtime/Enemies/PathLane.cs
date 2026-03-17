using System.Collections.Generic;
using TdRandomElemental.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TdRandomElemental.Enemies
{
    [AddComponentMenu("TD Random Elemental/Path Lane")]
    [DefaultExecutionOrder(-225)]
    public sealed class PathLane : MonoBehaviour
    {
        private const string RuntimeLaneObjectName = "__RuntimePathLane";

        private static readonly Vector3[] DefaultWaypointPositions =
        {
            new Vector3(-11f, 0f, 0f),
            new Vector3(-7f, 0f, 0f),
            new Vector3(-3f, 0f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(5f, 0f, 0f),
            new Vector3(9f, 0f, 0f),
            new Vector3(11f, 0f, 0f)
        };

        [Header("Lane")]
        [SerializeField] private List<Transform> waypoints = new List<Transform>();

        [Header("Debug")]
        [SerializeField] private EnemyDefinition debugEnemyDefinition;
        [SerializeField] private bool spawnDebugEnemyOnStart;

        public int WaypointCount => waypoints.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeLane()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != StartupConfig.MvpArenaSceneName)
            {
                return;
            }

            if (Object.FindFirstObjectByType<PathLane>() != null)
            {
                return;
            }

            GameObject laneRoot = new GameObject(RuntimeLaneObjectName);
            PathLane lane = laneRoot.AddComponent<PathLane>();
            lane.CreateDefaultWaypoints();
        }

        private void Awake()
        {
            RefreshWaypointsFromChildren();
        }

        private void Start()
        {
            if (!Application.isPlaying || !spawnDebugEnemyOnStart)
            {
                return;
            }

            if (Object.FindFirstObjectByType<EnemyMover>() != null)
            {
                return;
            }

            SpawnDebugEnemy();
        }

        public Vector3 GetSpawnPosition()
        {
            return WaypointCount > 0 ? waypoints[0].position : transform.position;
        }

        public Vector3 GetWaypointPosition(int waypointIndex)
        {
            if (waypointIndex < 0 || waypointIndex >= WaypointCount)
            {
                return GetSpawnPosition();
            }

            return waypoints[waypointIndex].position;
        }

        public bool TryGetWaypointPosition(int waypointIndex, out Vector3 position)
        {
            if (waypointIndex < 0 || waypointIndex >= WaypointCount)
            {
                position = Vector3.zero;
                return false;
            }

            position = waypoints[waypointIndex].position;
            return true;
        }

        [ContextMenu("Debug/Spawn Test Enemy")]
        public void SpawnDebugEnemy()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("PathLane debug enemy spawn requires Play Mode.", this);
                return;
            }

            GameObject enemyObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            enemyObject.name = debugEnemyDefinition != null
                ? $"Enemy_{debugEnemyDefinition.DisplayName}"
                : "Enemy_Debug";

            enemyObject.transform.position = GetSpawnPosition() + new Vector3(0f, 0.5f, 0f);

            EnemyHealth enemyHealth = enemyObject.AddComponent<EnemyHealth>();
            EnemyMover enemyMover = enemyObject.AddComponent<EnemyMover>();
            enemyObject.AddComponent<EnemyRewardOnDeath>();

            enemyHealth.Initialize(debugEnemyDefinition);
            enemyMover.Initialize(this, debugEnemyDefinition);
        }

        private void CreateDefaultWaypoints()
        {
            for (int i = 0; i < DefaultWaypointPositions.Length; i++)
            {
                GameObject waypoint = new GameObject($"Waypoint_{i + 1:00}");
                waypoint.transform.SetParent(transform, false);
                waypoint.transform.position = DefaultWaypointPositions[i];
            }

            RefreshWaypointsFromChildren();
        }

        private void RefreshWaypointsFromChildren()
        {
            waypoints.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                waypoints.Add(transform.GetChild(i));
            }
        }

        private void OnDrawGizmos()
        {
            if (WaypointCount == 0)
            {
                RefreshWaypointsFromChildren();
            }

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 1f);
            for (int i = 0; i < waypoints.Count; i++)
            {
                Transform waypoint = waypoints[i];
                if (waypoint == null)
                {
                    continue;
                }

                Gizmos.DrawSphere(waypoint.position, 0.25f);
                if (i + 1 < waypoints.Count && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoint.position, waypoints[i + 1].position);
                }
            }
        }
    }
}
