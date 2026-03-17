using System;
using System.Collections.Generic;
using TdRandomElemental.Enemies;
using UnityEngine;

namespace TdRandomElemental.Waves
{
    [Serializable]
    public struct WaveSpawnEntry
    {
        [SerializeField] private EnemyDefinition enemy;
        [Min(1)]
        [SerializeField] private int count;
        [Min(0f)]
        [SerializeField] private float initialDelay;
        [Min(0.01f)]
        [SerializeField] private float spawnInterval;

        public EnemyDefinition Enemy => enemy;
        public int Count => count;
        public float InitialDelay => initialDelay;
        public float SpawnInterval => spawnInterval;

        public WaveSpawnEntry(EnemyDefinition enemy, int count, float initialDelay, float spawnInterval)
        {
            this.enemy = enemy;
            this.count = Mathf.Max(1, count);
            this.initialDelay = Mathf.Max(0f, initialDelay);
            this.spawnInterval = Mathf.Max(0.01f, spawnInterval);
        }
    }

    [CreateAssetMenu(
        fileName = "WaveDefinition_",
        menuName = "TD Random Elemental/Data/Wave Definition",
        order = 30)]
    public sealed class WaveDefinition : ScriptableObject
    {
        [Min(1)]
        [SerializeField] private int waveIndex = 1;
        [Min(0f)]
        [SerializeField] private float preparationTime = 10f;
        [SerializeField] private bool isBossWave;
        [Min(0)]
        [SerializeField] private int clearGoldBonus;
        [SerializeField] private List<WaveSpawnEntry> spawnEntries = new List<WaveSpawnEntry>();

        public int WaveIndex => waveIndex;
        public float PreparationTime => preparationTime;
        public bool IsBossWave => isBossWave;
        public int ClearGoldBonus => clearGoldBonus;
        public IReadOnlyList<WaveSpawnEntry> SpawnEntries => spawnEntries;

        public void ApplyRuntimeData(
            int newWaveIndex,
            float newPreparationTime,
            bool newIsBossWave,
            int newClearGoldBonus,
            IEnumerable<WaveSpawnEntry> newSpawnEntries)
        {
            waveIndex = Mathf.Max(1, newWaveIndex);
            preparationTime = Mathf.Max(0f, newPreparationTime);
            isBossWave = newIsBossWave;
            clearGoldBonus = Mathf.Max(0, newClearGoldBonus);

            spawnEntries.Clear();
            if (newSpawnEntries == null)
            {
                return;
            }

            foreach (WaveSpawnEntry spawnEntry in newSpawnEntries)
            {
                spawnEntries.Add(spawnEntry);
            }
        }
    }
}
