using TdRandomElemental.Enemies;
using TdRandomElemental.Towers;
using UnityEngine;

namespace TdRandomElemental.Elements
{
    public static class ElementEffectProcessor
    {
        public static void ApplyHit(EnemyHealth target, TowerRuntime towerRuntime, Vector3 impactPoint)
        {
            if (target == null || target.IsDead || towerRuntime == null)
            {
                return;
            }

            target.ApplyDamage(towerRuntime.Damage);

            ElementDefinition elementDefinition = towerRuntime.ElementDefinition;
            if (elementDefinition == null || elementDefinition.PrimaryEffect == ElementEffectKind.None)
            {
                return;
            }

            switch (elementDefinition.PrimaryEffect)
            {
                case ElementEffectKind.Burn:
                    FireEffectHandler.Apply(target, elementDefinition);
                    break;

                case ElementEffectKind.Slow:
                    WaterEffectHandler.Apply(target, elementDefinition);
                    break;

                case ElementEffectKind.Knockback:
                    EarthEffectHandler.Apply(target, towerRuntime.transform.position, impactPoint, elementDefinition);
                    break;
            }
        }
    }
}
