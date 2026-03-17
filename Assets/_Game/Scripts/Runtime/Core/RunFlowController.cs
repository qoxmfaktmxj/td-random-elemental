using System;
using System.Collections.Generic;
using TdRandomElemental.Board;
using TdRandomElemental.Economy;
using TdRandomElemental.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TdRandomElemental.Core
{
    public enum RunOutcomeState
    {
        InProgress = 0,
        Won = 1,
        Lost = 2
    }

    [AddComponentMenu("TD Random Elemental/Run Flow Controller")]
    [DefaultExecutionOrder(-205)]
    public sealed class RunFlowController : MonoBehaviour
    {
        private const string RuntimeObjectName = "__RunFlowController";

        [Header("Input")]
        [SerializeField] private KeyCode restartKey = KeyCode.R;

        [Header("Boss Rules")]
        [Min(0)]
        [SerializeField] private int bossLockdownNodeCount = 2;

        [Header("Runtime State")]
        [SerializeField] private RunOutcomeState runOutcome = RunOutcomeState.InProgress;
        [SerializeField] private bool bossEncounterActive;
        [SerializeField] private List<TowerNode> lockedBossNodes = new List<TowerNode>();

        private bool bindingsEstablished;
        private bool restartTriggered;

        public static RunFlowController Instance { get; private set; }

        public event Action<RunOutcomeState> OutcomeChanged;
        public event Action<bool> BossEncounterChanged;

        public RunOutcomeState RunOutcome => runOutcome;
        public bool IsBossEncounterActive => bossEncounterActive;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeInstance()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != StartupConfig.MvpArenaSceneName)
            {
                return;
            }

            if (FindFirstObjectByType<RunFlowController>() != null)
            {
                return;
            }

            GameObject runtimeObject = new GameObject(RuntimeObjectName);
            runtimeObject.AddComponent<RunFlowController>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            runOutcome = RunOutcomeState.InProgress;
        }

        private void Start()
        {
            TryBindRuntimeDependencies();
        }

        private void Update()
        {
            TryBindRuntimeDependencies();

            if (restartTriggered || !Input.GetKeyDown(restartKey))
            {
                return;
            }

            RestartRun();
        }

        private void OnDestroy()
        {
            UnbindRuntimeDependencies();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        [ContextMenu("Debug/Restart Run")]
        private void DebugRestartRun()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("RunFlowController debug restart requires Play Mode.", this);
                return;
            }

            RestartRun();
        }

        private void TryBindRuntimeDependencies()
        {
            if (bindingsEstablished || RunStateService.Instance == null || WaveController.Instance == null)
            {
                return;
            }

            RunStateService.Instance.RunLost += HandleRunLost;
            WaveController.Instance.WaveStarted += HandleWaveStarted;
            WaveController.Instance.WaveCompleted += HandleWaveCompleted;
            WaveController.Instance.AllWavesCompleted += HandleAllWavesCompleted;
            bindingsEstablished = true;
        }

        private void UnbindRuntimeDependencies()
        {
            if (!bindingsEstablished)
            {
                return;
            }

            if (RunStateService.Instance != null)
            {
                RunStateService.Instance.RunLost -= HandleRunLost;
            }

            if (WaveController.Instance != null)
            {
                WaveController.Instance.WaveStarted -= HandleWaveStarted;
                WaveController.Instance.WaveCompleted -= HandleWaveCompleted;
                WaveController.Instance.AllWavesCompleted -= HandleAllWavesCompleted;
            }

            bindingsEstablished = false;
        }

        private void HandleRunLost()
        {
            SetRunOutcome(RunOutcomeState.Lost);
            EndBossEncounter();
            WaveController.Instance?.AbortWaveRun();
        }

        private void HandleWaveStarted(WaveDefinition waveDefinition)
        {
            if (waveDefinition == null || !waveDefinition.IsBossWave)
            {
                return;
            }

            BeginBossEncounter();
        }

        private void HandleWaveCompleted(WaveDefinition waveDefinition)
        {
            if (waveDefinition == null || !waveDefinition.IsBossWave)
            {
                return;
            }

            EndBossEncounter();
        }

        private void HandleAllWavesCompleted()
        {
            EndBossEncounter();
            SetRunOutcome(RunOutcomeState.Won);
        }

        private void BeginBossEncounter()
        {
            if (bossEncounterActive)
            {
                return;
            }

            bossEncounterActive = true;
            ApplyBossLockdown();
            BossEncounterChanged?.Invoke(true);
            Debug.Log("RunFlowController: Boss encounter started.", this);
        }

        private void EndBossEncounter()
        {
            if (!bossEncounterActive && lockedBossNodes.Count == 0)
            {
                return;
            }

            bossEncounterActive = false;
            ReleaseBossLockdown();
            BossEncounterChanged?.Invoke(false);
        }

        private void ApplyBossLockdown()
        {
            ReleaseBossLockdown();

            TowerBoardService boardService = TowerBoardService.Instance;
            if (boardService == null || bossLockdownNodeCount <= 0)
            {
                return;
            }

            IReadOnlyList<TowerNode> nodes = boardService.Nodes;
            if (nodes.Count == 0)
            {
                return;
            }

            int leftIndex = 0;
            int rightIndex = nodes.Count - 1;

            while (lockedBossNodes.Count < bossLockdownNodeCount && leftIndex <= rightIndex)
            {
                TryLockBossNode(nodes[leftIndex], boardService);
                leftIndex++;

                if (lockedBossNodes.Count >= bossLockdownNodeCount || rightIndex < leftIndex)
                {
                    continue;
                }

                TryLockBossNode(nodes[rightIndex], boardService);
                rightIndex--;
            }

            if (lockedBossNodes.Count == 0)
            {
                return;
            }

            Debug.Log($"RunFlowController: Boss locked {lockedBossNodes.Count} tower nodes.", this);
        }

        private void TryLockBossNode(TowerNode node, TowerBoardService boardService)
        {
            if (node == null || boardService == null || node.IsDisabled || node.IsOccupied)
            {
                return;
            }

            boardService.SetNodeDisabled(node, true);
            lockedBossNodes.Add(node);
        }

        private void ReleaseBossLockdown()
        {
            TowerBoardService boardService = TowerBoardService.Instance;
            if (boardService == null)
            {
                lockedBossNodes.Clear();
                return;
            }

            for (int i = 0; i < lockedBossNodes.Count; i++)
            {
                TowerNode node = lockedBossNodes[i];
                if (node != null)
                {
                    boardService.SetNodeDisabled(node, false);
                }
            }

            lockedBossNodes.Clear();
        }

        private void RestartRun()
        {
            restartTriggered = true;
            EndBossEncounter();
            SetRunOutcome(RunOutcomeState.InProgress);

            if (WaveController.Instance != null)
            {
                WaveController.Instance.AbortWaveRun();
            }

            RunStateService.Instance?.ResetForNewRun();
            SceneManager.LoadScene(StartupConfig.MvpArenaSceneName);
        }

        private void SetRunOutcome(RunOutcomeState outcome)
        {
            if (runOutcome == outcome)
            {
                return;
            }

            runOutcome = outcome;
            OutcomeChanged?.Invoke(runOutcome);
        }
    }
}
