using Unity.Entities;
using Unity.NetCode;

namespace Waddle.GameplayEffects.Data
{
    [GhostComponent]
    public struct GameplayEffect : IComponentData
    {
        public Entity Source;
        public Entity Target;
        public float Duration;
    }
}