using System;
using UnityEngine;

namespace TdRandomElemental.Waves
{
    [Serializable]
    public sealed class WaveRuntimeState
    {
        [SerializeField] private int totalWaveCount;
        [SerializeField] private int currentWaveIndex;
        [SerializeField] private int remainingToSpawn;
        [SerializeField] private int activeEnemyCount;
        [SerializeField] private bool isPreparing;
        [SerializeField] private bool isWaveRunning;
        [SerializeField] private float timeUntilNextWave;

        public int TotalWaveCount => totalWaveCount;
        public int CurrentWaveIndex => currentWaveIndex;
        public int RemainingToSpawn => remainingToSpawn;
        public int ActiveEnemyCount => activeEnemyCount;
        public int RemainingEnemyCount => remainingToSpawn + activeEnemyCount;
        public bool IsPreparing => isPreparing;
        public bool IsWaveRunning => isWaveRunning;
        public float TimeUntilNextWave => timeUntilNextWave;

        public void ConfigureRun(int waveCount)
        {
            totalWaveCount = Mathf.Max(0, waveCount);
            currentWaveIndex = 0;
            remainingToSpawn = 0;
            activeEnemyCount = 0;
            isPreparing = false;
            isWaveRunning = false;
            timeUntilNextWave = 0f;
        }

        public void BeginPreparation(int waveIndex, float preparationSeconds)
        {
            currentWaveIndex = Mathf.Max(1, waveIndex);
            isPreparing = true;
            isWaveRunning = false;
            remainingToSpawn = 0;
            activeEnemyCount = 0;
            timeUntilNextWave = Mathf.Max(0f, preparationSeconds);
        }

        public void SetPreparationTimer(float timeRemaining)
        {
            timeUntilNextWave = Mathf.Max(0f, timeRemaining);
        }

        public void BeginWave(int waveIndex, int totalSpawnCount)
        {
            currentWaveIndex = Mathf.Max(1, waveIndex);
            isPreparing = false;
            isWaveRunning = true;
            remainingToSpawn = Mathf.Max(0, totalSpawnCount);
            activeEnemyCount = 0;
            timeUntilNextWave = 0f;
        }

        public void RegisterEnemySpawned()
        {
            remainingToSpawn = Mathf.Max(0, remainingToSpawn - 1);
            activeEnemyCount++;
        }

        public void RegisterEnemyResolved()
        {
            activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);
        }

        public void FinishWave()
        {
            remainingToSpawn = 0;
            activeEnemyCount = 0;
            isPreparing = false;
            isWaveRunning = false;
            timeUntilNextWave = 0f;
        }
    }
}
