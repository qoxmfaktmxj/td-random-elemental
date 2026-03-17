using TdRandomElemental.Towers;
using UnityEngine;

namespace TdRandomElemental.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TowerRuntime))]
    public sealed class TowerWorldIndicator : MonoBehaviour
    {
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.75f, 0f);

        private TowerRuntime towerRuntime;
        private GUIStyle labelStyle;

        private void Awake()
        {
            towerRuntime = GetComponent<TowerRuntime>();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || Camera.main == null || towerRuntime == null)
            {
                return;
            }

            Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position + worldOffset);
            if (screenPoint.z <= 0f)
            {
                return;
            }

            EnsureStyle();

            string text = towerRuntime.BoundNode != null && towerRuntime.BoundNode.IsMergeCandidate
                ? $"T{towerRuntime.Tier}  Merge Ready"
                : $"T{towerRuntime.Tier}";

            Vector2 size = labelStyle.CalcSize(new GUIContent(text));
            Rect labelRect = new Rect(
                screenPoint.x - size.x * 0.5f,
                Screen.height - screenPoint.y - size.y * 0.5f,
                size.x,
                size.y);

            Color previousColor = labelStyle.normal.textColor;
            labelStyle.normal.textColor = towerRuntime.PrimaryColor;
            GUI.Label(labelRect, text, labelStyle);
            labelStyle.normal.textColor = previousColor;
        }

        private void EnsureStyle()
        {
            if (labelStyle != null)
            {
                return;
            }

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
        }
    }
}
