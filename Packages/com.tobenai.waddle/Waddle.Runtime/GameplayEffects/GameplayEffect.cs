using Unity.Entities;

namespace Waddle.Runtime.GameplayEffects
{
    public struct GameplayEffect : IComponentData
    {
        public bool IsInstant;
        public float Duration;
        public Entity Source;
        public Entity Target;
    }
}