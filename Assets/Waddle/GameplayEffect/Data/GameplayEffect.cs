using Unity.Entities;

namespace Waddle.GameplayEffect.Data
{
    public struct GameplayEffect : IBufferElementData
    {
        public Entity TargetEntity;
        public float Duration;
    }
}