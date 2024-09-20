using Unity.Entities;

namespace Waddle.Runtime.GameplayEffects
{
    public struct GameplayEffect : IComponentData
    {
        public Entity Source;
        public Entity Target;
        public float Duration;
    }
}