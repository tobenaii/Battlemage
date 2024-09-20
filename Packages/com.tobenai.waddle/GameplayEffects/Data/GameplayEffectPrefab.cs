using Unity.Entities;

namespace Waddle.GameplayEffects.Data
{
    public struct GameplayEffectPrefab : IComponentData
    {
        public Entity Value;
    }
}