using TdRandomElemental.Board;
using TdRandomElemental.Towers;

namespace TdRandomElemental.Summoning
{
    public sealed class TowerFactory
    {
        public TowerRuntime CreateTower(TowerNode node, TowerTierDefinition definition)
        {
            return TowerRuntime.SpawnAtNode(node, definition);
        }
    }
}
