using Unity.Entities;

namespace Waddle.GameplayEffects.Data
{
    public struct GameplayEffect : IComponentData
    {
        public Entity Source;
        public Entity Target;
        public float Duration;
    }
}