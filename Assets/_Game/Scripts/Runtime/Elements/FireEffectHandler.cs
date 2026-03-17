using TdRandomElemental.Enemies;

namespace TdRandomElemental.Elements
{
    public static class FireEffectHandler
    {
        public static void Apply(EnemyHealth target, ElementDefinition elementDefinition)
        {
            if (target == null || elementDefinition == null)
            {
                return;
            }

            GetOrAddStatusController(target).ApplyBurn(elementDefinition.StatusTuning, elementDefinition.PrimaryColor);
        }

        private static EnemyStatusController GetOrAddStatusController(EnemyHealth target)
        {
            EnemyStatusController controller = target.GetComponent<EnemyStatusController>();
            return controller != null ? controller : target.gameObject.AddComponent<EnemyStatusController>();
        }
    }
}
