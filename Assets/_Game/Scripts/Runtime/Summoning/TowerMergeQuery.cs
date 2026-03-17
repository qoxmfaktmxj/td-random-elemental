using System.Collections.Generic;
using TdRandomElemental.Board;
using TdRandomElemental.Towers;

namespace TdRandomElemental.Summoning
{
    public sealed class TowerMergeQuery
    {
        public List<TowerRuntime> CollectMatchingTowers(TowerRuntime referenceTower)
        {
            List<TowerRuntime> matchingTowers = new List<TowerRuntime>();
            if (referenceTower == null || TowerBoardService.Instance == null)
            {
                return matchingTowers;
            }

            matchingTowers.Add(referenceTower);

            IReadOnlyList<TowerNode> nodes = TowerBoardService.Instance.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                TowerNode node = nodes[i];
                if (node == null || !node.IsOccupied || node.Occupancy.OccupantObject == null)
                {
                    continue;
                }

                TowerRuntime candidateTower = node.Occupancy.OccupantObject.GetComponent<TowerRuntime>();
                if (candidateTower == null || ReferenceEquals(candidateTower, referenceTower))
                {
                    continue;
                }

                if (!HasSameMergeSignature(referenceTower, candidateTower))
                {
                    continue;
                }

                matchingTowers.Add(candidateTower);
            }

            return matchingTowers;
        }

        public bool CanMerge(TowerRuntime referenceTower)
        {
            if (referenceTower == null || referenceTower.IsMaxTier)
            {
                return false;
            }

            List<TowerRuntime> matchingTowers = CollectMatchingTowers(referenceTower);
            return matchingTowers.Count >= referenceTower.MergeCount;
        }

        private static bool HasSameMergeSignature(TowerRuntime left, TowerRuntime right)
        {
            return left != null
                && right != null
                && left.Tier == right.Tier
                && left.RoleId == right.RoleId
                && left.ElementId == right.ElementId;
        }
    }
}
