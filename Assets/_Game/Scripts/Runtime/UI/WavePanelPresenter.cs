using System.Text;
using TdRandomElemental.Waves;
using UnityEngine;

namespace TdRandomElemental.UI
{
    [DisallowMultipleComponent]
    public sealed class WavePanelPresenter : MonoBehaviour
    {
        public string BuildWaveSummary()
        {
            WaveController waveController = WaveController.Instance;
            if (waveController == null)
            {
                return "Wave: waiting for controller";
            }

            WaveRuntimeState runtimeState = waveController.RuntimeState;
            StringBuilder builder = new StringBuilder(160);
            builder.AppendLine($"Wave: {runtimeState.CurrentWaveIndex}/{runtimeState.TotalWaveCount}");

            if (runtimeState.IsPreparing)
            {
                builder.AppendLine(runtimeState.IsBossWave
                    ? $"Phase: Boss Prep ({runtimeState.TimeUntilNextWave:0.0}s)"
                    : $"Phase: Prepare ({runtimeState.TimeUntilNextWave:0.0}s)");
            }
            else if (runtimeState.IsWaveRunning)
            {
                builder.AppendLine(runtimeState.IsBossWave ? "Phase: Boss Combat" : "Phase: Combat");
            }
            else if (waveController.IsRunCompleted)
            {
                builder.AppendLine("Phase: Complete");
            }
            else
            {
                builder.AppendLine("Phase: Idle");
            }

            builder.AppendLine(runtimeState.IsBossWave ? "Threat: Boss" : "Threat: Standard");
            builder.AppendLine($"Remaining Spawn: {runtimeState.RemainingToSpawn}");
            builder.Append($"Active Enemies: {runtimeState.ActiveEnemyCount}");
            return builder.ToString();
        }
    }
}
