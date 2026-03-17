using System;
using System.Collections.Generic;
using TdRandomElemental.Towers;
using UnityEngine;

namespace TdRandomElemental.Summoning
{
    [Serializable]
    public sealed class SummonPool
    {
        [SerializeField] private List<TowerTierDefinition> towerDefinitions = new List<TowerTierDefinition>();

        public IReadOnlyList<TowerTierDefinition> TowerDefinitions => towerDefinitions;
        public int Count => towerDefinitions.Count;

        public void SetDefinitions(IEnumerable<TowerTierDefinition> definitions)
        {
            towerDefinitions.Clear();
            if (definitions == null)
            {
                return;
            }

            foreach (TowerTierDefinition definition in definitions)
            {
                if (definition != null)
                {
                    towerDefinitions.Add(definition);
                }
            }
        }

        public TowerTierDefinition RollRandomTower()
        {
            if (towerDefinitions.Count == 0)
            {
                return null;
            }

            int randomIndex = UnityEngine.Random.Range(0, towerDefinitions.Count);
            return towerDefinitions[randomIndex];
        }
    }
}
