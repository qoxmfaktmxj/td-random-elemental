using System;
using UnityEngine;

namespace TdRandomElemental.Board
{
    public enum TowerNodeState
    {
        Empty = 0,
        Occupied = 1,
        Disabled = 2
    }

    [Serializable]
    public sealed class TowerNodeOccupancy
    {
        [SerializeField] private TowerNodeState state = TowerNodeState.Empty;
        [SerializeField] private string occupantId;
        [SerializeField] private GameObject occupantObject;
        [SerializeField] private bool isMergeCandidate;

        public TowerNodeState State => state;
        public string OccupantId => occupantId;
        public GameObject OccupantObject => occupantObject;
        public bool IsMergeCandidate => isMergeCandidate;
        public bool IsEmpty => state == TowerNodeState.Empty;
        public bool IsOccupied => state == TowerNodeState.Occupied;
        public bool IsDisabled => state == TowerNodeState.Disabled;

        public void SetOccupied(string newOccupantId, GameObject newOccupantObject = null)
        {
            state = TowerNodeState.Occupied;
            occupantId = string.IsNullOrWhiteSpace(newOccupantId) ? "unknown_tower" : newOccupantId;
            occupantObject = newOccupantObject;
            isMergeCandidate = false;
        }

        public void Clear()
        {
            state = TowerNodeState.Empty;
            occupantId = string.Empty;
            occupantObject = null;
            isMergeCandidate = false;
        }

        public void SetDisabled(bool disabled)
        {
            if (disabled)
            {
                state = TowerNodeState.Disabled;
                occupantId = string.Empty;
                occupantObject = null;
                isMergeCandidate = false;
                return;
            }

            if (occupantObject != null || !string.IsNullOrWhiteSpace(occupantId))
            {
                state = TowerNodeState.Occupied;
                return;
            }

            state = TowerNodeState.Empty;
        }

        public void SetMergeCandidate(bool mergeCandidate)
        {
            isMergeCandidate = state == TowerNodeState.Occupied && mergeCandidate;
        }
    }
}
