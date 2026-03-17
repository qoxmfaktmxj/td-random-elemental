using System;
using System.Collections;
using System.Collections.Generic;
using TdRandomElemental.Core;
using TdRandomElemental.Economy;
using TdRandomElemental.Enemies;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TdRandomElemental.Waves
{
    [AddComponentMenu("TD Random Elemental/Wave Controller")]
    [DefaultExecutionOrder(-220)]
    [RequireComponent(typeof(EnemySpawner))]
    public sealed class WaveController : MonoBehaviour
    {
        private const string RuntimeWaveObjectName = "__WaveController";

        [Header("Wave Setup")]
        [SerializeField] private List<WaveDefinition> waveDefinitions = new List<WaveDefinition>();
        [SerializeField] private bool autoStart = true;
        [SerializeField] private bool useRuntimeFallbackWaves = true;

        [Header("Runtime State")]
        [SerializeField] private WaveRuntimeState runtimeState = new WaveRuntimeState();

        private readonly HashSet<int> trackedEnemyIds = new HashSet<int>();

        private EnemySpawner enemySpawner;
        private Coroutine waveRoutine;
        private bool runCompleted;

        public static WaveController Instance { get; private set; }

        public event Action<WaveDefinition> WaveStarted;
        public event Action<WaveDefinition> WaveCompleted;
        public event Action AllWavesCompleted;

        public WaveRuntimeState RuntimeState => runtimeState;
        public IReadOnlyList<WaveDefinition> WaveDefinitions => waveDefinitions;
        public bool IsRunCompleted => runCompleted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeWaveController()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != StartupConfig.MvpArenaSceneName)
            {
                return;
            }

            if (FindFirstObjectByType<WaveController>() != null)
            {
                return;
            }

            GameObject waveObject = new GameObject(RuntimeWaveObjectName);
            waveObject.AddComponent<EnemySpawner>();
            waveObject.AddComponent<WaveController>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            enemySpawner = GetComponent<EnemySpawner>();
            enemySpawner.EnemySpawned += HandleEnemySpawned;

            EnsureWaveDefinitions();
            runtimeState.ConfigureRun(waveDefinitions.Count);
        }

        private void Start()
        {
            if (!Application.isPlaying || !autoStart)
            {
                return;
            }

            StartWaveRun();
        }

        private void OnDestroy()
        {
            if (enemySpawner != null)
            {
                enemySpawner.EnemySpawned -= HandleEnemySpawned;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void StartWaveRun()
        {
            if (waveRoutine != null || waveDefinitions.Count == 0)
            {
                return;
            }

            trackedEnemyIds.Clear();
            runCompleted = false;
            runtimeState.ConfigureRun(waveDefinitions.Count);
            waveRoutine = StartCoroutine(RunWaveSequence());
        }

        [ContextMenu("Debug/Restart Wave Run")]
        private void DebugRestartWaveRun()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("WaveController debug restart requires Play Mode.", this);
                return;
            }

            if (waveRoutine != null)
            {
                StopCoroutine(waveRoutine);
                waveRoutine = null;
            }

            ClearActiveEnemies();
            StartWaveRun();
        }

        private IEnumerator RunWaveSequence()
        {
            for (int i = 0; i < waveDefinitions.Count; i++)
            {
                WaveDefinition waveDefinition = waveDefinitions[i];
                if (waveDefinition == null)
                {
                    continue;
                }

                runtimeState.BeginPreparation(waveDefinition.WaveIndex, waveDefinition.PreparationTime);
                yield return RunPreparationCountdown(waveDefinition.PreparationTime);

                int totalSpawnCount = CountWaveSpawns(waveDefinition);
                runtimeState.BeginWave(waveDefinition.WaveIndex, totalSpawnCount);
                WaveStarted?.Invoke(waveDefinition);

                yield return SpawnWaveEntries(waveDefinition);
                yield return WaitForWaveToClear();

                if (RunStateService.Instance != null && waveDefinition.ClearGoldBonus > 0)
                {
                    RunStateService.Instance.AddGold(waveDefinition.ClearGoldBonus);
                }

                runtimeState.FinishWave();
                WaveCompleted?.Invoke(waveDefinition);
            }

            waveRoutine = null;
            runCompleted = true;
            AllWavesCompleted?.Invoke();
        }

        private IEnumerator RunPreparationCountdown(float duration)
        {
            float remaining = Mathf.Max(0f, duration);
            runtimeState.SetPreparationTimer(remaining);

            while (remaining > 0f)
            {
                yield return null;
                remaining = Mathf.Max(0f, remaining - Time.deltaTime);
                runtimeState.SetPreparationTimer(remaining);
            }
        }

        private IEnumerator SpawnWaveEntries(WaveDefinition waveDefinition)
        {
            IReadOnlyList<WaveSpawnEntry> entries = waveDefinition.SpawnEntries;
            for (int i = 0; i < entries.Count; i++)
            {
                WaveSpawnEntry spawnEntry = entries[i];
                if (spawnEntry.InitialDelay > 0f)
                {
                    yield return new WaitForSeconds(spawnEntry.InitialDelay);
                }

                for (int spawnIndex = 0; spawnIndex < spawnEntry.Count; spawnIndex++)
                {
                    enemySpawner.SpawnEnemy(spawnEntry.Enemy);
                    if (spawnIndex + 1 < spawnEntry.Count)
                    {
                        yield return new WaitForSeconds(spawnEntry.SpawnInterval);
                    }
                }
            }
        }

        private IEnumerator WaitForWaveToClear()
        {
            while (runtimeState.RemainingEnemyCount > 0)
            {
                yield return null;
            }
        }

        private void HandleEnemySpawned(EnemyHealth enemyHealth, EnemyMover enemyMover)
        {
            if (enemyHealth == null)
            {
                return;
            }

            int enemyId = enemyHealth.GetInstanceID();
            trackedEnemyIds.Add(enemyId);
            runtimeState.RegisterEnemySpawned();

            enemyHealth.Died += HandleEnemyDied;
            if (enemyMover != null)
            {
                enemyMover.ReachedCore += HandleEnemyReachedCore;
            }
        }

        private void HandleEnemyDied(EnemyHealth enemyHealth)
        {
            EnemyMover enemyMover = enemyHealth != null ? enemyHealth.GetComponent<EnemyMover>() : null;
            ResolveEnemy(enemyHealth, enemyMover);
        }

        private void HandleEnemyReachedCore(EnemyMover enemyMover)
        {
            EnemyHealth enemyHealth = enemyMover != null ? enemyMover.GetComponent<EnemyHealth>() : null;
            ResolveEnemy(enemyHealth, enemyMover);
        }

        private void ResolveEnemy(EnemyHealth enemyHealth, EnemyMover enemyMover)
        {
            int trackedId = enemyHealth != null
                ? enemyHealth.GetInstanceID()
                : enemyMover != null
                    ? enemyMover.GetInstanceID()
                    : 0;

            if (trackedId == 0 || !trackedEnemyIds.Remove(trackedId))
            {
                return;
            }

            if (enemyHealth != null)
            {
                enemyHealth.Died -= HandleEnemyDied;
            }

            if (enemyMover != null)
            {
                enemyMover.ReachedCore -= HandleEnemyReachedCore;
            }

            runtimeState.RegisterEnemyResolved();
        }

        private void EnsureWaveDefinitions()
        {
            if (waveDefinitions.Count > 0 || !useRuntimeFallbackWaves)
            {
                return;
            }

            waveDefinitions = BuildRuntimeFallbackWaves();
        }

        private static int CountWaveSpawns(WaveDefinition waveDefinition)
        {
            int totalCount = 0;
            IReadOnlyList<WaveSpawnEntry> entries = waveDefinition.SpawnEntries;
            for (int i = 0; i < entries.Count; i++)
            {
                totalCount += Mathf.Max(0, entries[i].Count);
            }

            return totalCount;
        }

        private static List<WaveDefinition> BuildRuntimeFallbackWaves()
        {
            List<WaveDefinition> fallbackWaves = new List<WaveDefinition>(6);
            fallbackWaves.Add(CreateRuntimeWave(1, 2f, false, 2, new WaveSpawnEntry(null, 3, 0f, 0.75f)));
            fallbackWaves.Add(CreateRuntimeWave(2, 2f, false, 3, new WaveSpawnEntry(null, 4, 0f, 0.65f)));
            fallbackWaves.Add(CreateRuntimeWave(3, 2f, false, 4, new WaveSpawnEntry(null, 5, 0f, 0.55f)));
            fallbackWaves.Add(CreateRuntimeWave(4, 2f, false, 5, new WaveSpawnEntry(null, 6, 0f, 0.5f)));
            fallbackWaves.Add(CreateRuntimeWave(5, 2f, false, 6, new WaveSpawnEntry(null, 8, 0f, 0.45f)));
            fallbackWaves.Add(CreateRuntimeWave(6, 3f, true, 10, new WaveSpawnEntry(null, 1, 0f, 1f)));
            return fallbackWaves;
        }

        private static WaveDefinition CreateRuntimeWave(
            int waveIndex,
            float preparationTime,
            bool isBossWave,
            int clearGoldBonus,
            params WaveSpawnEntry[] entries)
        {
            WaveDefinition waveDefinition = ScriptableObject.CreateInstance<WaveDefinition>();
            waveDefinition.name = $"RuntimeWave_{waveIndex:00}";
            waveDefinition.ApplyRuntimeData(waveIndex, preparationTime, isBossWave, clearGoldBonus, entries);
            return waveDefinition;
        }

        private void ClearActiveEnemies()
        {
            trackedEnemyIds.Clear();
            EnemyHealth[] activeEnemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            for (int i = 0; i < activeEnemies.Length; i++)
            {
                if (activeEnemies[i] != null)
                {
                    Destroy(activeEnemies[i].gameObject);
                }
            }

            runtimeState.ConfigureRun(waveDefinitions.Count);
        }
    }
}
