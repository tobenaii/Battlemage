using Unity.Entities;

namespace Waddle.Abilities.Data
{
    public struct AbilityActivateRequest : IBufferElementData
    {
        public Entity AbilityPrefab;
    }
}