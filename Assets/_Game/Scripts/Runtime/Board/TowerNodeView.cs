using UnityEngine;

namespace TdRandomElemental.Board
{
    [DisallowMultipleComponent]
    public sealed class TowerNodeView : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color emptyColor = new Color(0.16f, 0.66f, 0.86f, 1f);
        [SerializeField] private Color occupiedColor = new Color(1f, 0.56f, 0.18f, 1f);
        [SerializeField] private Color disabledColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        [SerializeField] private Color mergeCandidateColor = new Color(0.92f, 0.2f, 0.78f, 1f);
        [SerializeField] private Color highlightedColor = new Color(1f, 0.92f, 0.3f, 1f);

        [Header("Scale")]
        [SerializeField] private Vector3 defaultScale = new Vector3(1.2f, 0.15f, 1.2f);
        [SerializeField] private float highlightedScaleMultiplier = 1.15f;

        private readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        private Renderer cachedRenderer;
        private TowerNode boundNode;
        private bool isHighlighted;

        private void Awake()
        {
            cachedRenderer = GetComponentInChildren<Renderer>();
            transform.localScale = defaultScale;
        }

        public void Bind(TowerNode node)
        {
            boundNode = node;
            Refresh();
        }

        public void SetHighlighted(bool highlighted)
        {
            if (isHighlighted == highlighted)
            {
                return;
            }

            isHighlighted = highlighted;
            Refresh();
        }

        public void Refresh()
        {
            if (cachedRenderer == null || boundNode == null)
            {
                return;
            }

            cachedRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", ResolveColor());
            propertyBlock.SetColor("_BaseColor", ResolveColor());
            cachedRenderer.SetPropertyBlock(propertyBlock);

            transform.localScale = isHighlighted
                ? defaultScale * highlightedScaleMultiplier
                : defaultScale;
        }

        private Color ResolveColor()
        {
            if (isHighlighted)
            {
                return highlightedColor;
            }

            if (boundNode.IsDisabled)
            {
                return disabledColor;
            }

            if (boundNode.IsMergeCandidate)
            {
                return mergeCandidateColor;
            }

            if (boundNode.IsOccupied)
            {
                return occupiedColor;
            }

            return emptyColor;
        }
    }
}
