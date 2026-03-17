using TdRandomElemental.Character;
using TdRandomElemental.Economy;
using TdRandomElemental.Summoning;
using TdRandomElemental.Towers;
using UnityEngine;

namespace TdRandomElemental.Board
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TowerNodeView))]
    [RequireComponent(typeof(Collider))]
    public sealed class TowerNode : MonoBehaviour, IPlayerInteractable
    {
        [SerializeField] private string nodeId = "node_00";
        [SerializeField] private TowerNodeOccupancy occupancy = new TowerNodeOccupancy();

        private TowerNodeView nodeView;

        public string NodeId => nodeId;
        public TowerNodeOccupancy Occupancy => occupancy;
        public bool IsOccupied => occupancy.IsOccupied;
        public bool IsEmpty => occupancy.IsEmpty;
        public bool IsDisabled => occupancy.IsDisabled;
        public bool IsMergeCandidate => occupancy.IsMergeCandidate;

        public Transform InteractionTransform => transform;

        public string InteractionPrompt
        {
            get
            {
                if (IsDisabled)
                {
                    return $"{nodeId} disabled";
                }

                if (IsOccupied)
                {
                    TowerRuntime towerRuntime = occupancy.OccupantObject != null
                        ? occupancy.OccupantObject.GetComponent<TowerRuntime>()
                        : null;

                    string displayName = towerRuntime != null
                        ? towerRuntime.DisplayName
                        : occupancy.OccupantId;

                    return IsMergeCandidate
                        ? $"E Merge / Q Sell {nodeId} ({displayName})"
                        : $"Q Sell {nodeId} ({displayName})";
                }

                int summonCost = RunStateService.Instance != null
                    ? RunStateService.Instance.BaseSummonCost
                    : 0;

                return $"E Summon {nodeId} ({summonCost}G)";
            }
        }

        private void Awake()
        {
            nodeView = GetComponent<TowerNodeView>();
            nodeView.Bind(this);
        }

        private void OnEnable()
        {
            TowerBoardService.Instance?.RegisterNode(this);
        }

        private void OnDisable()
        {
            if (TowerBoardService.Instance != null)
            {
                TowerBoardService.Instance.UnregisterNode(this);
            }
        }

        public void Configure(string newNodeId)
        {
            nodeId = newNodeId;
            RefreshView();
        }

        public void SetOccupied(string occupantId, GameObject occupantObject = null)
        {
            occupancy.SetOccupied(occupantId, occupantObject);
            RefreshView();
        }

        public void ClearOccupancy()
        {
            occupancy.Clear();
            RefreshView();
        }

        public void SetDisabled(bool disabled)
        {
            occupancy.SetDisabled(disabled);
            RefreshView();
        }

        public void SetMergeCandidate(bool mergeCandidate)
        {
            occupancy.SetMergeCandidate(mergeCandidate);
            RefreshView();
        }

        public bool CanInteract(GameObject interactor)
        {
            return !IsDisabled;
        }

        public void Interact(GameObject interactor)
        {
            if (SummonRequestHandler.Instance != null && SummonRequestHandler.Instance.HandleNodeInteraction(this, interactor))
            {
                return;
            }

            Debug.Log($"TowerNode '{nodeId}' interact. state={occupancy.State}, occupant={occupancy.OccupantId}", this);
        }

        public void SetHighlighted(bool highlighted)
        {
            if (nodeView == null)
            {
                nodeView = GetComponent<TowerNodeView>();
            }

            nodeView.SetHighlighted(highlighted);
        }

        private void RefreshView()
        {
            if (nodeView == null)
            {
                nodeView = GetComponent<TowerNodeView>();
            }

            nodeView.Refresh();
        }
    }
}
