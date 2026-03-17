using UnityEngine;

namespace TdRandomElemental.Towers
{
    public enum TowerRoleKind
    {
        Attack = 0,
        Control = 1,
        Impact = 2
    }

    public enum TowerTargetingMode
    {
        First = 0,
        Closest = 1,
        Strongest = 2
    }

    [CreateAssetMenu(
        fileName = "TowerRoleDefinition_",
        menuName = "TD Random Elemental/Data/Tower Role Definition",
        order = 10)]
    public sealed class TowerRoleDefinition : ScriptableObject
    {
        [SerializeField] private string roleId = "attack";
        [SerializeField] private string displayName = "Attack";
        [TextArea]
        [SerializeField] private string description = "Defines the core tower role.";
        [SerializeField] private TowerRoleKind roleKind = TowerRoleKind.Attack;
        [SerializeField] private TowerTargetingMode defaultTargetingMode = TowerTargetingMode.First;
        [SerializeField] private bool usesProjectile = true;

        public string RoleId => roleId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public TowerRoleKind RoleKind => roleKind;
        public TowerTargetingMode DefaultTargetingMode => defaultTargetingMode;
        public bool UsesProjectile => usesProjectile;

        public void ApplyRuntimeData(
            string newRoleId,
            string newDisplayName,
            string newDescription,
            TowerRoleKind newRoleKind,
            TowerTargetingMode newTargetingMode,
            bool newUsesProjectile)
        {
            roleId = string.IsNullOrWhiteSpace(newRoleId) ? "runtime_role" : newRoleId;
            displayName = string.IsNullOrWhiteSpace(newDisplayName) ? roleId : newDisplayName;
            description = newDescription ?? string.Empty;
            roleKind = newRoleKind;
            defaultTargetingMode = newTargetingMode;
            usesProjectile = newUsesProjectile;
        }
    }
}
