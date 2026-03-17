using TdRandomElemental.Enemies;
using UnityEngine;

namespace TdRandomElemental.Towers
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TowerRuntime))]
    public sealed class TowerTargetSelector : MonoBehaviour
    {
        private TowerRuntime towerRuntime;

        public EnemyHealth CurrentTarget { get; private set; }

        private void Awake()
        {
            towerRuntime = GetComponent<TowerRuntime>();
        }

        public EnemyHealth SelectTarget()
        {
            if (IsTargetValid(CurrentTarget))
            {
                return CurrentTarget;
            }

            EnemyHealth[] candidates = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            EnemyHealth bestTarget = null;
            float bestScore = float.MinValue;

            for (int i = 0; i < candidates.Length; i++)
            {
                EnemyHealth candidate = candidates[i];
                if (!IsTargetValid(candidate))
                {
                    continue;
                }

                float score = ScoreCandidate(candidate);
                if (score <= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestTarget = candidate;
            }

            CurrentTarget = bestTarget;
            return CurrentTarget;
        }

        private bool IsTargetValid(EnemyHealth candidate)
        {
            if (candidate == null || candidate.IsDead)
            {
                return false;
            }

            float range = towerRuntime != null ? towerRuntime.AttackRange : 0f;
            float sqrDistance = (candidate.transform.position - transform.position).sqrMagnitude;
            return sqrDistance <= range * range;
        }

        private float ScoreCandidate(EnemyHealth candidate)
        {
            switch (towerRuntime.TargetingMode)
            {
                case TowerTargetingMode.Closest:
                    return -Vector3.SqrMagnitude(candidate.transform.position - transform.position);

                case TowerTargetingMode.Strongest:
                    return candidate.CurrentHealth;

                case TowerTargetingMode.First:
                default:
                    EnemyMover mover = candidate.GetComponent<EnemyMover>();
                    float remainingDistance = mover != null ? mover.RemainingPathDistance : float.MaxValue;
                    return -remainingDistance;
            }
        }
    }
}
