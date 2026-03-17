using System;
using TdRandomElemental.Economy;
using UnityEngine;

namespace TdRandomElemental.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyHealth))]
    public sealed class EnemyMover : MonoBehaviour
    {
        [SerializeField] private EnemyDefinition definition;
        [SerializeField] private PathLane pathLane;
        [Min(0.1f)]
        [SerializeField] private float fallbackMoveSpeed = 2f;
        [Min(0.01f)]
        [SerializeField] private float reachThreshold = 0.2f;
        [SerializeField] private float speedMultiplier = 1f;
        [SerializeField] private Vector3 externalVelocity;
        [Min(0.1f)]
        [SerializeField] private float externalVelocityDamping = 10f;

        private int nextWaypointIndex;
        private bool hasReachedCore;

        public event Action<EnemyMover> ReachedCore;

        public EnemyDefinition Definition => definition;
        public PathLane PathLane => pathLane;
        public float RemainingPathDistance => CalculateRemainingPathDistance();

        private void Awake()
        {
            if (pathLane == null)
            {
                pathLane = FindFirstObjectByType<PathLane>();
            }
        }

        private void Start()
        {
            if (pathLane != null && nextWaypointIndex == 0)
            {
                transform.position = pathLane.GetSpawnPosition() + new Vector3(0f, 0.5f, 0f);
                nextWaypointIndex = pathLane.WaypointCount > 1 ? 1 : 0;
            }
        }

        private void Update()
        {
            if (pathLane == null || hasReachedCore || pathLane.WaypointCount == 0)
            {
                return;
            }

            if (!pathLane.TryGetWaypointPosition(nextWaypointIndex, out Vector3 targetPosition))
            {
                ReachCore();
                return;
            }

            Vector3 target = targetPosition + new Vector3(0f, 0.5f, 0f);
            Vector3 toTarget = target - transform.position;
            float moveDistance = GetMoveSpeed() * speedMultiplier * Time.deltaTime;

            if (toTarget.sqrMagnitude <= reachThreshold * reachThreshold)
            {
                nextWaypointIndex++;
                if (nextWaypointIndex >= pathLane.WaypointCount)
                {
                    ReachCore();
                }

                return;
            }

            Vector3 moveDirection = toTarget.normalized;
            Vector3 frameDisplacement = moveDirection * moveDistance + externalVelocity * Time.deltaTime;
            transform.position += frameDisplacement;

            externalVelocity = Vector3.MoveTowards(
                externalVelocity,
                Vector3.zero,
                externalVelocityDamping * Time.deltaTime);

            Vector3 facingDirection = frameDisplacement;
            facingDirection.y = 0f;
            if (facingDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(facingDirection.normalized, Vector3.up);
            }
        }

        public void Initialize(PathLane lane, EnemyDefinition enemyDefinition)
        {
            pathLane = lane;
            definition = enemyDefinition;
            nextWaypointIndex = pathLane != null && pathLane.WaypointCount > 1 ? 1 : 0;
            speedMultiplier = 1f;
            externalVelocity = Vector3.zero;

            transform.position = (pathLane != null ? pathLane.GetSpawnPosition() : transform.position) + new Vector3(0f, 0.5f, 0f);

            Vector3 modelScale = definition != null ? definition.ModelScale : Vector3.one;
            transform.localScale = modelScale;
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = Mathf.Clamp(multiplier, 0.1f, 1.5f);
        }

        public void ApplyImpulse(Vector3 impulseVelocity)
        {
            externalVelocity += impulseVelocity;
        }

        [ContextMenu("Debug/Teleport To Core")]
        private void DebugTeleportToCore()
        {
            if (pathLane == null || pathLane.WaypointCount == 0)
            {
                return;
            }

            transform.position = pathLane.GetWaypointPosition(pathLane.WaypointCount - 1) + new Vector3(0f, 0.5f, 0f);
            nextWaypointIndex = pathLane.WaypointCount;
            ReachCore();
        }

        private float GetMoveSpeed()
        {
            return definition != null ? definition.MoveSpeed : fallbackMoveSpeed;
        }

        private int GetCoreDamage()
        {
            return definition != null ? definition.CoreDamage : 1;
        }

        private float CalculateRemainingPathDistance()
        {
            if (pathLane == null || pathLane.WaypointCount == 0)
            {
                return float.MaxValue;
            }

            if (nextWaypointIndex >= pathLane.WaypointCount)
            {
                return 0f;
            }

            Vector3 currentPosition = transform.position;
            float totalDistance = 0f;

            if (pathLane.TryGetWaypointPosition(nextWaypointIndex, out Vector3 nextWaypointPosition))
            {
                Vector3 adjustedNextWaypoint = nextWaypointPosition + new Vector3(0f, 0.5f, 0f);
                totalDistance += Vector3.Distance(currentPosition, adjustedNextWaypoint);
            }

            for (int waypointIndex = nextWaypointIndex; waypointIndex + 1 < pathLane.WaypointCount; waypointIndex++)
            {
                Vector3 from = pathLane.GetWaypointPosition(waypointIndex) + new Vector3(0f, 0.5f, 0f);
                Vector3 to = pathLane.GetWaypointPosition(waypointIndex + 1) + new Vector3(0f, 0.5f, 0f);
                totalDistance += Vector3.Distance(from, to);
            }

            return totalDistance;
        }

        private void ReachCore()
        {
            if (hasReachedCore)
            {
                return;
            }

            hasReachedCore = true;
            if (RunStateService.Instance != null)
            {
                RunStateService.Instance.ApplyCoreDamage(GetCoreDamage());
            }

            ReachedCore?.Invoke(this);
            Debug.Log($"EnemyMover: '{name}' reached the core for {GetCoreDamage()} damage.", this);
            Destroy(gameObject);
        }
    }
}
