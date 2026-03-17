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
        private bool runAborted;
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
            runAborted = false;
            runCompleted = false;
            runtimeState.ConfigureRun(waveDefinitions.Count);
            waveRoutine = StartCoroutine(RunWaveSequence());
        }

        public void AbortWaveRun()
        {
            runAborted = true;

            if (waveRoutine != null)
            {
                StopCoroutine(waveRoutine);
                waveRoutine = null;
            }

            ClearActiveEnemies(false);
            runtimeState.FinishWave();
            runCompleted = false;
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

            ClearActiveEnemies(true);
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

                runtimeState.BeginPreparation(waveDefinition.WaveIndex, waveDefinition.PreparationTime, waveDefinition.IsBossWave);
                yield return RunPreparationCountdown(waveDefinition.PreparationTime);
                if (ShouldAbortRun())
                {
                    break;
                }

                int totalSpawnCount = CountWaveSpawns(waveDefinition);
                runtimeState.BeginWave(waveDefinition.WaveIndex, totalSpawnCount, waveDefinition.IsBossWave);
                WaveStarted?.Invoke(waveDefinition);

                yield return SpawnWaveEntries(waveDefinition);
                if (ShouldAbortRun())
                {
                    break;
                }

                yield return WaitForWaveToClear();
                if (ShouldAbortRun())
                {
                    break;
                }

                if (RunStateService.Instance != null && waveDefinition.ClearGoldBonus > 0)
                {
                    RunStateService.Instance.AddGold(waveDefinition.ClearGoldBonus);
                }

                runtimeState.FinishWave();
                WaveCompleted?.Invoke(waveDefinition);
            }

            waveRoutine = null;
            if (!ShouldAbortRun())
            {
                runCompleted = true;
                AllWavesCompleted?.Invoke();
            }
        }

        private IEnumerator RunPreparationCountdown(float duration)
        {
            float remaining = Mathf.Max(0f, duration);
            runtimeState.SetPreparationTimer(remaining);

            while (remaining > 0f)
            {
                if (ShouldAbortRun())
                {
                    yield break;
                }

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
                if (ShouldAbortRun())
                {
                    yield break;
                }

                WaveSpawnEntry spawnEntry = entries[i];
                if (spawnEntry.InitialDelay > 0f)
                {
                    yield return new WaitForSeconds(spawnEntry.InitialDelay);
                    if (ShouldAbortRun())
                    {
                        yield break;
                    }
                }

                for (int spawnIndex = 0; spawnIndex < spawnEntry.Count; spawnIndex++)
                {
                    if (ShouldAbortRun())
                    {
                        yield break;
                    }

                    enemySpawner.SpawnEnemy(spawnEntry.Enemy);
                    if (spawnIndex + 1 < spawnEntry.Count)
                    {
                        yield return new WaitForSeconds(spawnEntry.SpawnInterval);
                        if (ShouldAbortRun())
                        {
                            yield break;
                        }
                    }
                }
            }
        }

        private IEnumerator WaitForWaveToClear()
        {
            while (runtimeState.RemainingEnemyCount > 0)
            {
                if (ShouldAbortRun())
                {
                    yield break;
                }

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
            EnemyDefinition basicEnemy = CreateRuntimeEnemy("basic_enemy", "Shardling", EnemyArchetype.Basic, 12f, 2f, 1, 1, Vector3.one);
            EnemyDefinition fastEnemy = CreateRuntimeEnemy("fast_enemy", "Racer", EnemyArchetype.Fast, 9f, 3.1f, 1, 1, new Vector3(0.85f, 1.05f, 0.85f));
            EnemyDefinition tankEnemy = CreateRuntimeEnemy("tank_enemy", "Bulwark", EnemyArchetype.Tank, 26f, 1.45f, 2, 3, new Vector3(1.2f, 1.2f, 1.2f));
            EnemyDefinition bossEnemy = CreateRuntimeEnemy("boss_enemy", "Rift Colossus", EnemyArchetype.Boss, 140f, 1.25f, 5, 18, new Vector3(2.4f, 2.8f, 2.4f));

            fallbackWaves.Add(CreateRuntimeWave(1, 2f, false, 2, new WaveSpawnEntry(basicEnemy, 3, 0f, 0.75f)));
            fallbackWaves.Add(CreateRuntimeWave(2, 2f, false, 3, new WaveSpawnEntry(basicEnemy, 4, 0f, 0.65f), new WaveSpawnEntry(fastEnemy, 2, 0.5f, 0.55f)));
            fallbackWaves.Add(CreateRuntimeWave(3, 2f, false, 4, new WaveSpawnEntry(fastEnemy, 6, 0f, 0.48f)));
            fallbackWaves.Add(CreateRuntimeWave(4, 2f, false, 5, new WaveSpawnEntry(tankEnemy, 3, 0f, 0.8f), new WaveSpawnEntry(basicEnemy, 4, 0.35f, 0.45f)));
            fallbackWaves.Add(CreateRuntimeWave(5, 2f, false, 6, new WaveSpawnEntry(tankEnemy, 4, 0f, 0.72f), new WaveSpawnEntry(fastEnemy, 4, 0.35f, 0.38f)));
            fallbackWaves.Add(CreateRuntimeWave(6, 3f, true, 14, new WaveSpawnEntry(bossEnemy, 1, 0f, 1f)));
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

        private static EnemyDefinition CreateRuntimeEnemy(
            string enemyId,
            string displayName,
            EnemyArchetype archetype,
            float maxHealth,
            float moveSpeed,
            int coreDamage,
            int goldReward,
            Vector3 modelScale)
        {
            EnemyDefinition definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            definition.name = $"RuntimeEnemy_{displayName}";
            definition.ApplyRuntimeData(enemyId, displayName, archetype, maxHealth, moveSpeed, coreDamage, goldReward, modelScale);
            return definition;
        }

        private bool ShouldAbortRun()
        {
            return runAborted || (RunStateService.Instance != null && RunStateService.Instance.IsRunLost);
        }

        private void ClearActiveEnemies(bool resetRuntimeState)
        {
            trackedEnemyIds.Clear();
            EnemyHealth[] activeEnemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            for (int i = 0; i < activeEnemies.Length; i++)
            {
                if (activeEnemies[i] == null)
                {
                    continue;
                }

                EnemyMover enemyMover = activeEnemies[i].GetComponent<EnemyMover>();
                activeEnemies[i].Died -= HandleEnemyDied;
                if (enemyMover != null)
                {
                    enemyMover.ReachedCore -= HandleEnemyReachedCore;
                }

                if (activeEnemies[i] != null)
                {
                    Destroy(activeEnemies[i].gameObject);
                }
            }

            if (resetRuntimeState)
            {
                runtimeState.ConfigureRun(waveDefinitions.Count);
            }
        }
    }
}
