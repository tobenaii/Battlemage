using Unity.Entities;

namespace Waddle.Runtime.GameplayEffects
{
    public struct GameplayEffectPrefab : IComponentData
    {
        public Entity Value;
    }
}