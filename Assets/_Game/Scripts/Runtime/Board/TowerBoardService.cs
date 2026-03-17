using System.Collections.Generic;
using System;
using TdRandomElemental.Core;
using TdRandomElemental.Towers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TdRandomElemental.Board
{
    [AddComponentMenu("TD Random Elemental/Tower Board Service")]
    [DefaultExecutionOrder(-250)]
    public sealed class TowerBoardService : MonoBehaviour
    {
        private const string RuntimeBoardObjectName = "__TowerBoardService";
        private const string RuntimeNodeRootName = "__RuntimeTowerNodes";

        private static readonly Vector3[] DefaultNodePositions =
        {
            new Vector3(-8f, 0f, -4f),
            new Vector3(-4f, 0f, -4f),
            new Vector3(0f, 0f, -4f),
            new Vector3(4f, 0f, -4f),
            new Vector3(8f, 0f, -4f),
            new Vector3(-8f, 0f, 4f),
            new Vector3(-4f, 0f, 4f),
            new Vector3(0f, 0f, 4f),
            new Vector3(4f, 0f, 4f),
            new Vector3(8f, 0f, 4f)
        };

        private readonly List<TowerNode> nodes = new List<TowerNode>();

        public static TowerBoardService Instance { get; private set; }

        public IReadOnlyList<TowerNode> Nodes => nodes;
        public event Action BoardChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimeBoard()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != StartupConfig.MvpArenaSceneName)
            {
                return;
            }

            if (UnityEngine.Object.FindFirstObjectByType<TowerBoardService>() != null)
            {
                return;
            }

            GameObject boardObject = new GameObject(RuntimeBoardObjectName);
            boardObject.AddComponent<TowerBoardService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            RegisterExistingNodes();
            if (nodes.Count == 0)
            {
                SpawnDefaultNodes();
                RegisterExistingNodes();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void RegisterNode(TowerNode node)
        {
            if (node == null || nodes.Contains(node))
            {
                return;
            }

            nodes.Add(node);
        }

        public void UnregisterNode(TowerNode node)
        {
            if (node == null)
            {
                return;
            }

            nodes.Remove(node);
        }

        public bool TryOccupyNode(TowerNode node, string occupantId, GameObject occupantObject = null)
        {
            if (!IsNodeAvailableForOccupancy(node))
            {
                return false;
            }

            node.SetOccupied(occupantId, occupantObject);
            Debug.Log($"TowerBoardService: Occupied '{node.NodeId}' with '{occupantId}'.", this);
            BoardChanged?.Invoke();
            return true;
        }

        public bool ClearNode(TowerNode node)
        {
            if (node == null || node.IsEmpty)
            {
                return false;
            }

            DestroyOccupantObject(node);
            node.ClearOccupancy();
            Debug.Log($"TowerBoardService: Cleared '{node.NodeId}'.", this);
            BoardChanged?.Invoke();
            return true;
        }

        public void SetNodeDisabled(TowerNode node, bool disabled)
        {
            if (node == null)
            {
                return;
            }

            if (disabled)
            {
                DestroyOccupantObject(node);
            }

            node.SetDisabled(disabled);
            Debug.Log($"TowerBoardService: Node '{node.NodeId}' disabled={disabled}.", this);
            BoardChanged?.Invoke();
        }

        public void SetMergeCandidate(TowerNode node, bool mergeCandidate)
        {
            if (node == null)
            {
                return;
            }

            node.SetMergeCandidate(mergeCandidate);
        }

        public bool IsNodeAvailableForOccupancy(TowerNode node)
        {
            return node != null && !node.IsDisabled && node.IsEmpty;
        }

        public TowerNode GetFirstAvailableNode()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (IsNodeAvailableForOccupancy(nodes[i]))
                {
                    return nodes[i];
                }
            }

            return null;
        }

        [ContextMenu("Debug/Occupy First Empty Node")]
        private void DebugOccupyFirstEmptyNode()
        {
            TowerNode availableNode = GetFirstAvailableNode();
            if (availableNode == null)
            {
                Debug.Log("TowerBoardService Debug: No empty node was available.", this);
                return;
            }

            TryOccupyNode(availableNode, $"debug_tower_{availableNode.NodeId}");
        }

        [ContextMenu("Debug/Spawn Attack Tower On First Empty Node")]
        private void DebugSpawnAttackTower()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("TowerBoardService debug tower spawn requires Play Mode.", this);
                return;
            }

            TowerNode availableNode = GetFirstAvailableNode();
            if (availableNode == null)
            {
                Debug.Log("TowerBoardService Debug: No empty node was available for a tower.", this);
                return;
            }

            TowerRuntime towerRuntime = TowerRuntime.SpawnDebugTower(availableNode);
            if (towerRuntime == null)
            {
                Debug.LogWarning("TowerBoardService Debug: Failed to spawn debug tower.", this);
                return;
            }

            Debug.Log($"TowerBoardService Debug: Spawned '{towerRuntime.DisplayName}' on '{availableNode.NodeId}'.", this);
        }

        [ContextMenu("Debug/Clear All Nodes")]
        private void DebugClearAllNodes()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                ClearNode(nodes[i]);
            }

            Debug.Log("TowerBoardService Debug: Cleared all node occupancy.", this);
        }

        [ContextMenu("Debug/Mark First Three Occupied As Merge")]
        private void DebugMarkMergeCandidates()
        {
            int markedCount = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                bool shouldMark = nodes[i].IsOccupied && markedCount < 3;
                nodes[i].SetMergeCandidate(shouldMark);
                if (shouldMark)
                {
                    markedCount++;
                }
            }

            Debug.Log($"TowerBoardService Debug: Marked {markedCount} merge candidate nodes.", this);
        }

        [ContextMenu("Debug/Toggle First Node Disabled")]
        private void DebugToggleFirstNodeDisabled()
        {
            if (nodes.Count == 0)
            {
                Debug.Log("TowerBoardService Debug: No nodes registered.", this);
                return;
            }

            TowerNode node = nodes[0];
            node.SetDisabled(!node.IsDisabled);
            Debug.Log($"TowerBoardService Debug: First node disabled={node.IsDisabled}.", this);
        }

        private void RegisterExistingNodes()
        {
            nodes.Clear();
            TowerNode[] existingNodes = FindObjectsByType<TowerNode>(FindObjectsSortMode.None);
            for (int i = 0; i < existingNodes.Length; i++)
            {
                RegisterNode(existingNodes[i]);
            }
        }

        private void SpawnDefaultNodes()
        {
            GameObject nodeRoot = new GameObject(RuntimeNodeRootName);
            nodeRoot.transform.SetParent(transform, false);

            for (int i = 0; i < DefaultNodePositions.Length; i++)
            {
                GameObject nodeObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                nodeObject.name = $"TowerNode_{i + 1:00}";
                nodeObject.transform.SetParent(nodeRoot.transform, false);
                nodeObject.transform.position = DefaultNodePositions[i];

                TowerNode node = nodeObject.AddComponent<TowerNode>();
                node.Configure($"node_{i + 1:00}");
            }
        }

        private static void DestroyOccupantObject(TowerNode node)
        {
            if (node == null || node.Occupancy.OccupantObject == null)
            {
                return;
            }

            UnityEngine.Object.Destroy(node.Occupancy.OccupantObject);
        }
    }
}
