using TdRandomElemental.Economy;
using UnityEngine;

namespace TdRandomElemental.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyHealth))]
    public sealed class EnemyRewardOnDeath : MonoBehaviour
    {
        [Min(0)]
        [SerializeField] private int fallbackGoldReward = 1;

        private EnemyHealth enemyHealth;

        private void Awake()
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }

        private void OnEnable()
        {
            if (enemyHealth != null)
            {
                enemyHealth.Died += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (enemyHealth != null)
            {
                enemyHealth.Died -= HandleDied;
            }
        }

        private void HandleDied(EnemyHealth deadEnemy)
        {
            int goldReward = deadEnemy.Definition != null
                ? deadEnemy.Definition.GoldReward
                : fallbackGoldReward;

            if (RunStateService.Instance != null && goldReward > 0)
            {
                RunStateService.Instance.AddGold(goldReward);
            }

            Debug.Log($"EnemyRewardOnDeath: '{deadEnemy.name}' dropped {goldReward} gold.", this);
            Destroy(deadEnemy.gameObject);
        }
    }
}
