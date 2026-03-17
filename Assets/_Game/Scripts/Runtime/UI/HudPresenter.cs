using System.Text;
using TdRandomElemental.Character;
using TdRandomElemental.Core;
using TdRandomElemental.Economy;
using TdRandomElemental.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TdRandomElemental.UI
{
    [AddComponentMenu("TD Random Elemental/UI/Hud Presenter")]
    [DefaultExecutionOrder(200)]
    [RequireComponent(typeof(WavePanelPresenter))]
    [RequireComponent(typeof(InteractPromptPresenter))]
    public sealed class HudPresenter : MonoBehaviour
    {
        private const string RuntimeObjectName = "__HudPresenter";

        [Header("Layout")]
        [SerializeField] private Vector2 statusPanelSize = new Vector2(360f, 190f);
        [SerializeField] private Vector2 controlsPanelSize = new Vector2(300f, 128f);
        [SerializeField] private Vector2 panelMargin = new Vector2(18f, 18f);
        [SerializeField] private Vector2 promptPanelSize = new Vector2(520f, 64f);

        [Header("Palette")]
        [SerializeField] private Color panelBackground = new Color(0.08f, 0.1f, 0.13f, 0.92f);
        [SerializeField] private Color panelAccent = new Color(0.95f, 0.64f, 0.2f, 1f);
        [SerializeField] private Color bodyText = new Color(0.92f, 0.95f, 0.98f, 1f);
        [SerializeField] private Color successText = new Color(0.4f, 1f, 0.72f, 1f);
        [SerializeField] private Color warningText = new Color(1f, 0.75f, 0.3f, 1f);
        [SerializeField] private Color failText = new Color(1f, 0.42f, 0.42f, 1f);

        private WavePanelPresenter wavePanelPresenter;
        private InteractPromptPresenter interactPromptPresenter;

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle promptStyle;
        private GUIStyle stateStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeInstance()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != StartupConfig.MvpArenaSceneName)
            {
                return;
            }

            if (FindFirstObjectByType<HudPresenter>() != null)
            {
                return;
            }

            GameObject runtimeObject = new GameObject(RuntimeObjectName);
            runtimeObject.AddComponent<WavePanelPresenter>();
            runtimeObject.AddComponent<InteractPromptPresenter>();
            runtimeObject.AddComponent<HudPresenter>();
        }

        private void Awake()
        {
            wavePanelPresenter = GetComponent<WavePanelPresenter>();
            interactPromptPresenter = GetComponent<InteractPromptPresenter>();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            EnsureStyles();
            DrawStatusPanel();
            DrawControlsPanel();
            DrawInteractPrompt();
            DrawRunStateBanner();
        }

        private void DrawStatusPanel()
        {
            Rect panelRect = new Rect(panelMargin.x, panelMargin.y, statusPanelSize.x, statusPanelSize.y);
            GUI.Box(panelRect, GUIContent.none, panelStyle);

            GUILayout.BeginArea(new Rect(panelRect.x + 14f, panelRect.y + 10f, panelRect.width - 28f, panelRect.height - 20f));
            GUILayout.Label("RUN STATUS", titleStyle);

            StringBuilder builder = new StringBuilder(256);
            if (RunStateService.Instance != null)
            {
                builder.AppendLine($"Gold: {RunStateService.Instance.GoldWallet.CurrentGold}");
                builder.AppendLine($"Core HP: {RunStateService.Instance.CoreHealth.CurrentHealth}/{RunStateService.Instance.CoreHealth.MaxHealth}");
                builder.AppendLine($"Pity: {RunStateService.Instance.PityGauge.CurrentValue:0}/{RunStateService.Instance.PityGauge.MaxValue:0}");
                builder.AppendLine($"Summon Cost: {RunStateService.Instance.BaseSummonCost}G");
            }
            else
            {
                builder.AppendLine("Run state unavailable");
            }

            string wavePanelText = wavePanelPresenter != null ? wavePanelPresenter.BuildWaveSummary() : "Wave info unavailable";
            builder.Append(wavePanelText);

            GUILayout.Label(builder.ToString(), bodyStyle);
            GUILayout.EndArea();
        }

        private void DrawControlsPanel()
        {
            Rect panelRect = new Rect(
                Screen.width - controlsPanelSize.x - panelMargin.x,
                panelMargin.y,
                controlsPanelSize.x,
                controlsPanelSize.y);

            GUI.Box(panelRect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(panelRect.x + 14f, panelRect.y + 10f, panelRect.width - 28f, panelRect.height - 20f));
            GUILayout.Label("CONTROLS", titleStyle);
            GUILayout.Label("WASD  Move\nShift  Dash\nE  Summon / Merge\nQ  Sell\nR  Restart Run", bodyStyle);
            GUILayout.EndArea();
        }

        private void DrawInteractPrompt()
        {
            if (interactPromptPresenter == null)
            {
                return;
            }

            string prompt = interactPromptPresenter.CurrentPrompt;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return;
            }

            Rect panelRect = new Rect(
                (Screen.width - promptPanelSize.x) * 0.5f,
                Screen.height - promptPanelSize.y - 24f,
                promptPanelSize.x,
                promptPanelSize.y);

            GUI.Box(panelRect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(panelRect.x + 14f, panelRect.y + 12f, panelRect.width - 28f, panelRect.height - 24f));
            GUILayout.Label(prompt, promptStyle);
            GUILayout.EndArea();
        }

        private void DrawRunStateBanner()
        {
            string bannerText = string.Empty;
            Color bannerColor = successText;

            RunFlowController runFlowController = RunFlowController.Instance;
            if (runFlowController != null)
            {
                switch (runFlowController.RunOutcome)
                {
                    case RunOutcomeState.Lost:
                        bannerText = "CORE DESTROYED\nPress R to Restart";
                        bannerColor = failText;
                        break;

                    case RunOutcomeState.Won:
                        bannerText = "BOSS DEFEATED\nPress R to Restart";
                        bannerColor = successText;
                        break;

                    default:
                        if (runFlowController.IsBossEncounterActive)
                        {
                            bannerText = "BOSS WAVE";
                            bannerColor = warningText;
                        }

                        break;
                }
            }
            else if (RunStateService.Instance != null && RunStateService.Instance.IsRunLost)
            {
                bannerText = "CORE DESTROYED";
                bannerColor = failText;
            }
            else if (WaveController.Instance != null && WaveController.Instance.IsRunCompleted)
            {
                bannerText = "WAVES CLEARED";
                bannerColor = successText;
            }

            if (string.IsNullOrEmpty(bannerText))
            {
                return;
            }

            Color previousColor = stateStyle.normal.textColor;
            stateStyle.normal.textColor = bannerColor;

            Rect bannerRect = new Rect((Screen.width - 420f) * 0.5f, 26f, 420f, 58f);
            GUI.Box(bannerRect, GUIContent.none, panelStyle);
            GUI.Label(bannerRect, bannerText, stateStyle);

            stateStyle.normal.textColor = previousColor;
        }

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            Texture2D panelTexture = MakeTexture(panelBackground);

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = panelTexture },
                border = new RectOffset(2, 2, 2, 2),
                padding = new RectOffset(10, 10, 10, 10)
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = panelAccent }
            };

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                richText = false,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = bodyText }
            };

            promptStyle = new GUIStyle(bodyStyle)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = warningText }
            };

            stateStyle = new GUIStyle(bodyStyle)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private static Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
