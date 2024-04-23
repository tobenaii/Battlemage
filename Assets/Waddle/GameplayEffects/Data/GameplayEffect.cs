using Unity.Entities;

namespace Waddle.GameplayEffects.Data
{
    public struct GameplayEffect : IBufferElementData
    {
        public Entity TargetEntity;
        public float Duration;
    }
}